using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;
using Orchard;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Transactions;
using Orchard.Messaging.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Data;
using NKD.Module.BusinessObjects;
using NKD.Services;
using Orchard.Media.Services;
using EXPEDIT.Share.ViewModels;
using EXPEDIT.Share.Helpers;
using Orchard.DisplayManagement;
using ImpromptuInterface;
using NKD.Models;
using NKD.Helpers;
using System.Drawing;
using System.Web.Hosting;
using Orchard.Environment.Configuration;

namespace EXPEDIT.Share.Services {
    
    [UsedImplicitly]
    public class ShareService : IShareService {

        private const string DIRECTORY_TEMP = "EXPEDIT.Share\\Temp";
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IStorageProvider _storage;
        private ShellSettings _settings;
        private readonly IUsersService _users;
        private readonly IMediaService _media;
        public ILogger Logger { get; set; }

        public ShareService(
            IContentManager contentManager, 
            IOrchardServices orchardServices, 
            IMessageManager messageManager, 
            IScheduledTaskManager taskManager,
            IStorageProvider storage,
            ShellSettings shellSettings,
            IUsersService users, 
            IMediaService media)
        {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _messageManager = messageManager;
            _taskManager = taskManager;
            _storage = storage;
            _settings = shellSettings;
            _media = media;
            _users = users;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
      

        public string GetRedirect(string routeURL)
        {
            try
            {
                var application = _users.ApplicationID;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    var route = (from o in d.ApplicationRoutes where o.ApplicationID == application && o.RouteURL == routeURL orderby o.Sequence descending select o).FirstOrDefault();
                    var routeTable = d.GetTableName(route.GetType());
                    if (route.IsCapturingStatistic.HasValue && route.IsCapturingStatistic.Value)
                    {
                        var stat = (from o in d.StatisticDatas
                                    where o.ReferenceID == route.ApplicationRouteID && o.TableType == routeTable
                                    && o.StatisticDataName == ConstantsHelper.STAT_NAME_ROUTES
                                    select o).FirstOrDefault();
                        if (stat == null)
                        {
                            stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = routeTable, ReferenceID = route.ApplicationRouteID, StatisticDataName = ConstantsHelper.STAT_NAME_ROUTES, Count = 0 };
                            d.StatisticDatas.AddObject(stat);
                        }
                        stat.Count++;
                        d.SaveChanges();
                    }
                    if (route.IsExternal.HasValue && route.IsExternal.Value)
                        return route.RedirectURL;
                    else
                        return VirtualPathUtility.ToAbsolute(route.RedirectURL);
                }
            }
            catch
            {
                return null;
            }
        }

        public FileData GetDownload(string downloadID, string requestIPAddress)
        {
            try
            {        
                var application = _users.ApplicationID;
                var contact = _users.ContactID;                
                var company = _users.ApplicationCompanyID;
                var server = _users.ServerID;                
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    var download = (from o in d.Downloads where o.DownloadID==new Guid(downloadID) select o).First();
                    var downloadTable = d.GetTableName(download.GetType());
                    if (download.FilterApplicationID.HasValue && download.FilterApplicationID.Value != application)
                        throw new OrchardSecurityException(T("Application {0} not permitted to download {1}", application, downloadID));
                    if (download.FilterContactID.HasValue && download.FilterContactID.Value != contact)
                        throw new OrchardSecurityException(T("Contact {0} not permitted to download {1}", contact, downloadID));
                    if (download.FilterCompanyID.HasValue && download.FilterCompanyID.Value != company)
                        throw new OrchardSecurityException(T("Company {0} not permitted to download {1}", company, downloadID));
                    if (download.FilterServerID.HasValue && download.FilterServerID.Value != company)
                        throw new OrchardSecurityException(T("Server {0} not permitted to upload {1}", server, downloadID));
                    if (!string.IsNullOrWhiteSpace(download.FilterClientIP) && string.Format("{0}", requestIPAddress).Trim().ToUpperInvariant() != download.FilterClientIP.Trim().ToUpperInvariant())
                        throw new OrchardSecurityException(T("IP {0} not permitted to download {1}", requestIPAddress, downloadID));
                    if (download.RemainingDownloads < 1)
                        throw new OrchardSecurityException(T("No more remaining downloads for {0}.", downloadID));
                    if (download.ValidFrom.HasValue && download.ValidFrom.Value > DateTime.Now)
                        throw new OrchardSecurityException(T("Download {0} not yet valid.", downloadID));
                    if (download.ValidUntil.HasValue && download.ValidUntil.Value < DateTime.Now)
                        throw new OrchardSecurityException(T("Download {0} no longer valid.", downloadID));
                    var stat = (from o in d.StatisticDatas
                                where o.ReferenceID == download.DownloadID && o.TableType == downloadTable
                                && o.StatisticDataName == ConstantsHelper.STAT_NAME_DOWNLOADS
                                select o).FirstOrDefault();
                    if (stat == null)
                    {
                        stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = downloadTable, ReferenceID = download.DownloadID, StatisticDataName = ConstantsHelper.STAT_NAME_DOWNLOADS, Count = 0 };
                        d.StatisticDatas.AddObject(stat);
                    }
                    stat.Count++;
                    FileData file;
                    if (!string.IsNullOrWhiteSpace(download.FileChecksum))
                        file = d.FileDatas.First(f => f.VersionAntecedentID == download.FileDataID && f.FileChecksum == download.FileChecksum); //version aware download
                    else
                        file = d.FileDatas.First(f => f.FileDataID == download.FileDataID);
                    if (file != null)
                        download.RemainingDownloads--;
                    d.SaveChanges();
                    return file;
                }
            }
            catch
            {
                return null;
            }
        }

        internal bool CheckFilePrivileges(NKDC d, Guid fileDataID)
        {
            var table = d.GetTableName(typeof(FileData));
            var root = (from o in d.FileDatas where o.FileDataID == fileDataID && o.Version == 0 && o.VersionDeletedBy == null select new { o.VersionAntecedentID, o.VersionOwnerCompanyID, o.VersionOwnerContactID }).FirstOrDefault();
            var verified = false;
            if (root == null)
                return false;
            else if (!root.VersionOwnerCompanyID.HasValue && !root.VersionOwnerContactID.HasValue)
                verified = true;
            else if (root.VersionAntecedentID.HasValue)
                verified = _users.CheckPermission(new SecuredBasic
                {
                    AccessorApplicationID = _users.ApplicationID,
                    AccessorContactID = _users.ContactID,
                    OwnerReferenceID = root.VersionAntecedentID.Value,
                    OwnerTableType = table
                }, NKD.Models.ActionPermission.Read);
            else
                verified = _users.CheckPermission(new SecuredBasic
                {
                    AccessorApplicationID = _users.ApplicationID,
                    AccessorContactID = _users.ContactID,
                    OwnerReferenceID = fileDataID,
                    OwnerTableType = table
                }, NKD.Models.ActionPermission.Read);
            if (!verified)
                throw new AuthorityException(string.Format("Can not download file: {0} Unauthorised access by contact: {1}", fileDataID, _users.ContactID));
            var stat = (from o in d.StatisticDatas
                        where o.ReferenceID == fileDataID && o.TableType == table
                        && o.StatisticDataName == ConstantsHelper.STAT_NAME_DOWNLOADS
                        select o).FirstOrDefault();
            if (stat == null)
            {
                stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = table, ReferenceID = fileDataID, StatisticDataName = ConstantsHelper.STAT_NAME_DOWNLOADS, Count = 0 };
                d.StatisticDatas.AddObject(stat);
            }
            stat.Count++;
            d.SaveChanges();
            return true;
        }

        public FileData GetFile(Guid fileDataID)
        {
            try
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    if (!CheckFilePrivileges(d, fileDataID))
                        return null;
                    var cacheKey = string.Format("{0}-{1}", fileDataID, CacheHelper.CacheType.Original);
                    var file = CacheHelper.Cache[cacheKey] as FileData;
                    if (file == null) 
                        return CacheHelper.AddFileDataToCache((from o in d.FileDatas where o.FileDataID == fileDataID && o.Version == 0 && o.VersionDeletedBy == null select o).Single()
                            , cacheKey); 
                    else
                        return file;
                }
            }
            catch
            {
                return null;
            }
        }

       

        /// <summary>
        /// Always return 200x200 Png
        /// </summary>
        /// <param name="fileDataID"></param>
        /// <returns></returns>
        public FileData GetPreview(Guid fileDataID)
        {
            try
            {

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    if (!CheckFilePrivileges(d, fileDataID))
                        return null;
                    var cacheKey = string.Format("{0}-{1}", fileDataID, CacheHelper.CacheType.Preview);
                    var file = CacheHelper.Cache[cacheKey] as FileData;
                    if (file == null) {
                        var preview = (from o in d.FileDatas where o.FileDataID == fileDataID && o.Version == 0 && o.VersionDeletedBy == null select o).Single();
                        if (preview.MimeType != null && preview.MimeType.ToLower().StartsWith("image"))
                        {
                            using (var thumb = new MemoryStream())
                            {
                                using (var full = new MemoryStream(preview.FileBytes))
                                {
                                    try
                                    {
                                        Image image = Image.FromStream(full);
                                        Image tn = image.GetThumbnailImage(200, 200, () => false, IntPtr.Zero);
                                        tn.Save(thumb, System.Drawing.Imaging.ImageFormat.Png);
                                        preview.FileBytes = thumb.ToArray();
                                        if (preview.FileBytes.Length < 1)
                                            preview.FileBytes = null;
                                    }
                                    catch
                                    {
                                        preview.FileBytes = null;
                                    }
                                }
                            }
                        }
                        else
                            preview.FileBytes = null;
                        preview.FileContent = null;
                        return CacheHelper.AddFileDataToCache(preview, cacheKey);

                    }
                    else
                        return file;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns search within NKD. Only search products for now.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="supplierModelID"></param>
        /// <param name="startRowIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<IHtmlString> GetSearchResults(string text = null, Guid? supplierModelID = null, int? startRowIndex = null, int? pageSize = null)
        {
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _media.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(SupplierModel));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, null, null, null, null, null, null, null, null, null, null, null, null, null,null, null, startRowIndex, pageSize, verified, found)
                        select GetSearchResultShape(new SearchViewModel
                        {
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Sequence = o.Row,
                            Total = o.TotalRows,
                            UrlInternal = o.InternalURL
                        })
                       ).ToArray();
            }
        }

        /// <summary>
        /// Returns search files within NKD. Only search products for now.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="supplierModelID"></param>
        /// <param name="startRowIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<SearchViewModel> GetFiles(string text = null, int? startRowIndex = null, int? pageSize = null)
        {
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _media.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(FileData));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startRowIndex, pageSize, verified, found)
                        select new SearchViewModel
                        {
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Sequence = o.Row,
                            Total = o.TotalRows,
                            UrlInternal = o.InternalURL,
                            UrlExternal = o.ExternalURL,
                            SpatialJSON = o.SpatialJSON,
                            Score = o.Score,
                            Author = o.Author,
                            Updated = o.Updated
                        }
                       ).ToArray();
            }
        }


        [Shape]
        public IHtmlString GetSearchResultShape(SearchViewModel model)
        {
            return new HtmlString(string.Format("<a href='/share/go/{0}'><h2>{1}</h2>{2}</a>", model.UrlInternal, model.Title, model.Description)); //TODO:Extend search different object types
        }


        public bool SubmitFiles(Dictionary<Guid, HttpPostedFileBase> files, Dictionary<Guid, int> fileLengths )
        {
            var contact = _users.ContactID;
            var company = _users.DefaultContactCompanyID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);               
                if (files != null)
                {
                    var mediaPath = HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/Media/") ?? "" : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                    var storagePath = Path.Combine(mediaPath, _settings.Name);
                    foreach (var f in files)
                    {
                        var filename = string.Concat(f.Value.FileName.Reverse().Take(50).Reverse());
                        var table = d.GetTableName(typeof(Contact));
                        var file = new FileData
                        {
                            FileDataID = f.Key,
                            TableType = table,
                            ReferenceID = contact,
                            FileTypeID = null, //TODO give type
                            FileName = filename,
                            FileLength = f.Value.ContentLength,
                            MimeType = f.Value.ContentType,
                            VersionOwnerContactID = contact,
                            VersionOwnerCompanyID = company,
                            VersionUpdated = DateTime.UtcNow,
                            VersionAntecedentID = f.Key,
                            VersionUpdatedBy = contact,
                            DocumentType = ConstantsHelper.DOCUMENT_TYPE_CONTENT_SUBMISSION
                        };
                        fileLengths.Add(f.Key, f.Value.ContentLength);
                        _media.GetMediaFolders(DIRECTORY_TEMP);
                        var path = string.Format("{0}\\{1}-{2}-{3}", DIRECTORY_TEMP, string.Format("{0}", contact).Replace("-", ""), f.Key.ToString().Replace("-", "").Substring(15), filename.ToString().Replace("-", ""));
                        var sf = _storage.CreateFile(path);
                        using (var sw = sf.OpenWrite())
                            f.Value.InputStream.CopyTo(sw);
                        f.Value.InputStream.Close();
                        try
                        {

                            using (var dh = new DocHelper.FilterReader(Path.Combine(storagePath, path)))
                                file.FileContent = dh.ReadToEnd();
                        }
                        catch { }
                        using (var sr = sf.OpenRead())
                            file.FileBytes = sr.ToByteArray();
                        _storage.DeleteFile(path);
                        file.FileChecksum = file.FileBytes.ComputeHash();
                        d.FileDatas.AddObject(file);
                        d.SaveChanges(); //Commit after each file

                    }
                }


            }

            return true;
        }

        public IEnumerable<SearchViewModel> GetLocations(string text = null, int? startRowIndex = null, int? pageSize = null)
        {
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _media.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(Location));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startRowIndex, pageSize, verified, found)
                        select new SearchViewModel
                        {
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Sequence = o.Row,
                            Total = o.TotalRows,
                            UrlInternal = o.InternalURL,
                            UrlExternal = o.ExternalURL,
                            SpatialJSON = o.SpatialJSON,
                            Score = o.Score,
                            Author = o.Author,
                            Updated = o.Updated
                        }
                       ).ToArray();
            }
        }

        public PickLocationViewModel GetLocation(Guid locationID)
        {
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _media.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(Location));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.Locations
                        where o.LocationID == locationID
                        select new PickLocationViewModel
                        {
                            ReferenceID = o.LocationID,
                            TableType = table,
                            LocationName = o.DefaultLocationName,
                            Comment = o.Comment,
                            Geography = o.LocationGeography.AsText(),
                            LocationID = locationID,
                            CountryID = o.CountryID,
                            CountryName = o.Country.StandardCountryName,
                            CountryStateID = o.CountryStateID,
                            CountryStateName = o.CountryState.StandardCountryStateName,
                            LocationCode = o.LocationCode,
                            PostCode = o.Postcode
                        }
                       ).FirstOrDefault();
            }
        }

        public bool SubmitLocation(Guid id, string locationName, string geography, string culture = "en-US")
        {
            return SubmitLocation(new PickLocationViewModel
            {
                LocationID = id,
                LocationName = locationName,
                Geography = geography,
                Culture = culture
            });
        }
        public bool SubmitLocation(PickLocationViewModel m)
        {
            if (string.IsNullOrWhiteSpace(m.LocationName))
                return false;
            else
                m.LocationName = m.LocationName.Capitalize();
            var contact = _users.ContactID;
            var company = _users.DefaultContactCompanyID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                Microsoft.SqlServer.Types.OpenGisGeographyType gType;
                var geo = NKD.Helpers.SpatialHelper.CreateGeography(m.Geography, out gType).GetCentre();
                var isNew = !d.Locations.Any(f => f.LocationID == m.LocationID);
                if (!isNew && d.Locations.Any(f => f.LocationGeography.SpatialEquals(geo) && f.VersionOwnerCompanyID == company && f.LocationID != m.LocationID))
                    return false;
                if (string.IsNullOrWhiteSpace(m.CountryID))
                    m.CountryID = (from o in d.Provinces where o.ProvinceGeography.Intersects(geo) && o.CountryID != null select o.CountryID).FirstOrDefault();
                var table = d.GetTableName(typeof(Location));
                if (!_users.CheckPermission(new SecuredBasic
                {
                    AccessorApplicationID = _users.ApplicationID,
                    AccessorContactID = _users.ContactID,
                    OwnerReferenceID = m.LocationID,
                    OwnerTableType = table
                }, isNew ? NKD.Models.ActionPermission.Create : ActionPermission.Update))
                    return false;


                Location location;

                if (isNew)
                {
                    location = new Location
                    {
                        LocationID = m.LocationID.Value,
                        LocationTypeID = ConstantsHelper.LOCATION_TYPE_UNCLASSIFIED,
                        VersionCertainty = -11,
                        VersionOwnerContactID = contact,
                        VersionOwnerCompanyID = company,
                    };
                    d.Locations.AddObject(location);
                }
                else
                {
                    location = (from o in d.Locations where o.LocationID == m.LocationID && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                    if (location == null || location.VersionOwnerCompanyID != company)
                        return false;
                }

                if (location.DefaultLocationName != m.LocationName)
                    location.DefaultLocationName = m.LocationName;
                if (location.LocationGeography.AsText() != geo.AsText())
                    location.LocationGeography = geo;
                if (location.CountryID != m.CountryID)
                    location.CountryID = m.CountryID;
                if (location.LongitudeWGS84 != (decimal)geo.Longitude.Value)
                    location.LongitudeWGS84 = (decimal)geo.Longitude.Value;
                if (location.LatitudeWGS84 != (decimal)geo.Latitude.Value)
                    location.LatitudeWGS84 = (decimal)geo.Latitude.Value;
                if (location.Postcode != m.PostCode)
                    location.Postcode = m.PostCode;
                if (location.Comment != m.Comment)
                    location.Comment = m.Comment;
                if (location.EntityState != System.Data.EntityState.Unchanged)
                {
                    location.VersionUpdated = DateTime.UtcNow;
                    location.VersionAntecedentID = m.LocationID;
                    location.VersionUpdatedBy = contact;
                }
                d.SaveChanges();

            }
            return true;
        }
       
    }
}
