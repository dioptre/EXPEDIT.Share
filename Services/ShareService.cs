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
using Orchard.MediaLibrary.Services;
using EXPEDIT.Share.ViewModels;
using EXPEDIT.Share.Helpers;
using Orchard.DisplayManagement;
using ImpromptuInterface;
using NKD.Models;
using NKD.Helpers;
using System.Drawing;
using System.Web.Hosting;
using Orchard.Environment.Configuration;
using NKD.ViewModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using EntityFramework.Extensions;

namespace EXPEDIT.Share.Services {
    
    [UsedImplicitly]
    public class ShareService : IShareService {

        private const string DIRECTORY_TEMP = "EXPEDIT.Share\\Temp";
        private readonly IOrchardServices _orchardServices;
        private readonly IStorageProvider _storage;
        private ShellSettings _settings;
        private readonly IUsersService _users;
        private readonly IMediaLibraryService _media;
        public ILogger Logger { get; set; }

        public ShareService(
            IOrchardServices orchardServices, 
            IStorageProvider storage,
            ShellSettings shellSettings,
            IUsersService users,
            IMediaLibraryService media)
        {
            _orchardServices = orchardServices;
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
        public FileData GetPreview(Guid fileDataID, int width = 200, int? height = 200, bool crop = false, ImageHelper.ImageFormat format = ImageHelper.ImageFormat.jpeg)
        {
            try
            {
                if (width < 1 || width > 600 || fileDataID == default(Guid))
                    return null;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    if (!CheckFilePrivileges(d, fileDataID))
                        return null;
                    var cacheKey = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", fileDataID, width, height, crop, format, CacheHelper.CacheType.Preview);
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
                                        Image tn;
                                        if (height == default(int?))
                                        {

                                            tn = image.Resize(width,  (int)((double)width / (double)image.Width * (double)image.Height), crop);
                                        }
                                        else
                                            tn = image.Resize(width, height.Value, crop);
                                        //Image tn = image.GetThumbnailImage(200, 200, () => false, IntPtr.Zero);
                                        switch (format)
                                        {
                                            case ImageHelper.ImageFormat.jpeg:
                                                tn.Save(thumb, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                break;
                                            case ImageHelper.ImageFormat.gif:
                                                tn.Save(thumb, System.Drawing.Imaging.ImageFormat.Gif);
                                                break;
                                            case ImageHelper.ImageFormat.png:
                                                tn.Save(thumb, System.Drawing.Imaging.ImageFormat.Png);
                                                break;
                                            default:
                                                return null;
                                        }
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

        public byte[] GetPhoto(Guid userID)
        {
            try
            {

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null, false);
                    var cacheKey = string.Format("{0}-PHOTO-{1}", userID, CacheHelper.CacheType.Preview);
                    var file = CacheHelper.Cache[cacheKey] as byte[];
                    if (file == null)
                    {
                        var preview = (from o in d.Contacts where o.AspNetUserID == userID && o.Version == 0 && o.VersionDeletedBy == null select o).Single();
                        if (preview.Photo!= null && preview.Photo.Length > 0)
                        using (var thumb = new MemoryStream())
                        {
                            using (var full = new MemoryStream(preview.Photo))
                            {
                                try
                                {
                                    Image image = Image.FromStream(full);
                                    Image tn = image.Resize(200, 200, true);
                                    tn.Save(thumb, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    file = thumb.ToArray();

                                }
                                catch
                                {
                                    file = null;
                                }
                            }
                            return CacheHelper.AddToCache<byte[]>(() => { return file; }, cacheKey, new TimeSpan(0, 15, 0));    
                        }
                        
                    }
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
        public IEnumerable<IHtmlString> GetSearchResults(string text = null, Guid? supplierModelID = null, int? startRowIndex = null, int? pageSize = null, string filterCategories = null)
        {
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _storage.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(SupplierModel));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, ConstantsHelper.DEVICE_TYPE_SOFTWARE, filterCategories, null, null, null, null, null, null, null, null, null, null, null, null,null, null, null, startRowIndex, pageSize, verified, found)
                        select GetSearchResultShape(new SearchViewModel
                        {
                            id = o.id,
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Row = o.Row,
                            TotalRows = o.TotalRows,
                            InternalUrl = o.InternalURL
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
            //if (string.IsNullOrWhiteSpace(text))
            //    return new SearchViewModel[] { };
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _storage.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(FileData));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startRowIndex, pageSize, verified, found)
                        select new SearchViewModel
                        {
                            id = o.id,
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Row = o.Row,
                            TotalRows = o.TotalRows,
                            InternalUrl = o.InternalURL,
                            ExternalUrl = o.ExternalURL,
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
            return new HtmlString(string.Format("<a href='/share/go/{0}'><h2>{1}</h2>{2}</a>", model.InternalUrl, model.Title, model.Description)); //TODO:Extend search different object types
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
                    Guid? creatorContact, creatorCompany;
                    _users.GetCreator(contact, company, out creatorContact, out creatorCompany);
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
                            VersionOwnerContactID = creatorContact,
                            VersionOwnerCompanyID = creatorCompany,
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
            if (string.IsNullOrWhiteSpace(text))
                return new SearchViewModel[] { };
            var contact = _users.ContactID;
            var application = _users.ApplicationID;
            var directory = _storage.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(Location));
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(int));
                var found = new System.Data.Objects.ObjectParameter("found", typeof(int));
                return (from o in d.E_SP_GetSecuredSearch(text, contact, application, table, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startRowIndex, pageSize, verified, found)
                        select new SearchViewModel
                        {
                            id = o.id,
                            ReferenceID = o.ReferenceID,
                            TableType = o.TableType,
                            Title = o.Title,
                            Description = o.Description,
                            Row = o.Row,
                            TotalRows = o.TotalRows,
                            InternalUrl = o.InternalURL,
                            ExternalUrl = o.ExternalURL,
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
            var directory = _storage.GetPublicUrl(@"EXPEDIT.Transactions");
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var table = d.GetTableName(typeof(Location));
                bool verified = _users.CheckPermission(new SecuredBasic
                {
                    AccessorApplicationID = application,
                    AccessorContactID = contact,
                    OwnerReferenceID = locationID,
                    OwnerTableType = table
                }, NKD.Models.ActionPermission.Read);
                if (!verified)
                    return null;
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

        public bool SubmitForm(MyFormViewModel m)
        {
            try
            {
                var contact = _users.ContactID;
                var company = _users.DefaultContactCompanyID;
                m.FormData = JsonConvert.SerializeObject(m.Form.FormData);              
                m.id = Guid.Parse(m.Form.FormJSON.id.Value);
                m.FormOrigin = m.Form.FormJSON.origin.Value;

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null);
                    Form theForm = d.Forms.Single(f=> f.FormID == m.id && f.VersionDeletedBy == null && f.Version == 0);

                    if (!string.IsNullOrWhiteSpace(theForm.FormActions))
                    {
                        string trs = ""; //m.FormData
                        foreach (var tuple in m.Form.FormData) 
                            trs += string.Format("<tr><td><strong>{0}</strong></td><td>{1}</td></tr>", tuple.name.Value, tuple.value.Value);
                        var username = "";
                        if (_orchardServices.WorkContext.CurrentUser != null)
                            username = _orchardServices.WorkContext.CurrentUser.UserName;
                        var body = string.Format(ConstantsHelper.FORM_BODY_TEMPLATE, theForm.FormName, trs, username, DateTime.UtcNow, m.id);
                        _users.EmailUsers(
                            theForm.FormActions.Split(','),
                            string.Format("{0} Form Submitted ({1})", theForm.FormName, m.id),
                            body);
                    }

                    var formData = new FormData {
                        FormID = m.FormID, 
                        FormDataID = Guid.NewGuid(), 
                        FormOrigin = m.FormOrigin, 
                        FormContent = m.FormData,
                        VersionOwnerContactID = contact,
                        VersionOwnerCompanyID = company,
                        VersionUpdated = DateTime.UtcNow,
                        VersionUpdatedBy = contact
                    };
                    theForm.FormDatas.Add(formData);
                    d.SaveChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public IEnumerable<MyFormViewModel> GetFormResults(Guid formID)
        {
            var isAdmin = _orchardServices.Authorizer.Authorize(EXPEDIT.Share.Permissions.FormBuilder) || _orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner) || _orchardServices.Authorizer.Authorize(StandardPermissions.AccessAdminPanel);
            var contact = _users.ContactID;
            var company = _users.DefaultContactCompanyID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var m = (from o in d.FormDatas
                         join f in d.Forms on o.FormID equals f.FormID
                         join c in d.Contacts on o.VersionOwnerContactID equals c.ContactID 
                         into jc
                        from contacts in jc.DefaultIfEmpty()
                            where o.FormID == formID && (f.VersionOwnerContactID == contact || o.VersionOwnerContactID == contact || isAdmin) && o.VersionDeletedBy == null && o.Version == 0
                            select new MyFormViewModel
                            {
                                FormData = o.FormContent,
                                Updated = o.VersionUpdated,
                                UpdatedBy = contacts.Username 
                            });
                return m.ToArray();
            }

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
                if (isNew)
                {
                    if (!_users.CheckPermission(new SecuredBasic
                    {
                        AccessorApplicationID = _users.ApplicationID,
                        AccessorContactID = _users.ContactID,
                        OwnerTableType = table
                    }, NKD.Models.ActionPermission.Create))
                        return false;
                }
                else
                {
                    if (!_users.CheckPermission(new SecuredBasic
                    {
                        AccessorApplicationID = _users.ApplicationID,
                        AccessorContactID = _users.ContactID,
                        OwnerReferenceID = m.LocationID,
                        OwnerTableType = table
                    }, ActionPermission.Update))
                        return false;
                }


                Location location;

                if (isNew)
                {
                    Guid? creatorContact, creatorCompany;
                    _users.GetCreator(contact, company, out creatorContact, out creatorCompany);
                    location = new Location
                    {
                        LocationID = m.LocationID.Value,
                        LocationTypeID = ConstantsHelper.LOCATION_TYPE_UNCLASSIFIED,
                        VersionCertainty = -11,
                        VersionOwnerContactID = creatorContact,
                        VersionOwnerCompanyID = creatorCompany,
                    };
                    d.Locations.AddObject(location);
                }
                else
                {
                    location = (from o in d.Locations where o.LocationID == m.LocationID && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                    if (location == null || (location.VersionOwnerCompanyID != company && location.VersionCertainty > -2)) //Only allow edits of own data
                        return false;
                }

                if (isNew || location.DefaultLocationName != m.LocationName)
                    location.DefaultLocationName = m.LocationName;
                if (isNew || location.LocationGeography.AsText() != geo.AsText())
                    location.LocationGeography = geo;
                if (isNew || location.CountryID != m.CountryID)
                    location.CountryID = m.CountryID;
                if (isNew || location.LongitudeWGS84 != (decimal)geo.Longitude.Value)
                    location.LongitudeWGS84 = (decimal)geo.Longitude.Value;
                if (isNew || location.LatitudeWGS84 != (decimal)geo.Latitude.Value)
                    location.LatitudeWGS84 = (decimal)geo.Latitude.Value;
                if (isNew || location.Postcode != m.PostCode)
                    location.Postcode = m.PostCode;
                if (isNew || location.Comment != m.Comment)
                    location.Comment = m.Comment;
                if (isNew || location.EntityState != System.Data.EntityState.Unchanged)
                {
                    location.VersionUpdated = DateTime.UtcNow;
                    location.VersionAntecedentID = m.LocationID;
                    location.VersionUpdatedBy = contact;
                }
                d.SaveChanges();

            }
            return true;
        }

        public ContactViewModel GetMyInfo()
        {
            return _users.GetMyInfo();
        }

        public bool VerifyUserUnicity(string userName, string email)
        {
            return _users.VerifyUserUnicity(userName, email);
        }


        public bool RequestLostPassword(string username)
        {
            return _users.RequestLostPassword(username);
        }

        public IUser SignUp(UserSignupViewModel m)
        {
            if (!m.CaptchaCookie.HasValue)
            {
                m.Response = SignupResponse.NoCaptcha;
                return null;
            }
            if (!ValidateCaptcha(m.CaptchaCookie.Value, m.CaptchaKey)) {
                m.Response = SignupResponse.BadCaptcha;
                return null;
            }
            return _users.Create(m.Email, m.UserName, m.Password);
        }

        public bool ValidateCaptcha(Guid cookie, string key)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var m = (from o in d.MetaDatas where o.MetaDataID == cookie select o).FirstOrDefault();
                if (m == null)
                {
                    return false;
                }
                else
                {
                    if (m.ContentToIndex != key)
                        return false;
                    else
                    {
                        d.DeleteObject(m);
                        d.SaveChanges();
                        return true;
                    }
                }              

            }
        }

        public CaptchaViewModel RequestCaptcha(Guid cookie)
        {            
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var m = (from o in d.MetaDatas where o.MetaDataID == cookie select o).FirstOrDefault();
                var lastCheck = DateTime.Now.AddHours(-2);
                string k = null;
                if (m == null)
                {
                    k = CaptchaHelper.CreateRandomText(4);
                    m = new MetaData
                    {
                        ContentToIndex = k,
                        MetaDataID = cookie,
                        MetaDataType = ConstantsHelper.METADATA_CAPTCHA,
                        VersionUpdated = DateTime.UtcNow

                    };
                    d.MetaDatas.AddObject(m);
                    (from o in d.MetaDatas where o.MetaDataType == ConstantsHelper.METADATA_CAPTCHA && o.VersionUpdated < lastCheck select o).Delete();
                    d.SaveChanges();
                }
                else
                {
                    k = m.ContentToIndex;
                }

                string img = null;
                using (var ms = new MemoryStream())
                {
                    new CaptchaHelper(k, 100, 40, FontFamily.GenericSansSerif).Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    img = Convert.ToBase64String(ms.ToArray());
                }
                return new CaptchaViewModel
                {
                    Cookie = cookie,
                    IsValid = false,
                    Image64 = img
                };

            } 
        }

        public bool SubmitPhoto(HttpPostedFileBase file)
        {
            if (!file.ContentType.ToLower().StartsWith("image"))
                return false;
            var img = file.InputStream.ToByteArray();
            if (img == null || img.Length < 1)
                return false;
            var contact = _users.ContactID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                
                var d = new NKDC(_users.ApplicationConnectionString, null);
                if (file != null && contact.HasValue)
                {
                    var m = (from o in d.Contacts where o.ContactID == contact && o.Version==0 && o.VersionDeletedBy == null select o).Single();
                    m.Photo = img;                    
                    d.SaveChanges();
                    var cacheKey = string.Format("{0}-PHOTO-{1}", m.AspNetUserID, CacheHelper.CacheType.Preview);
                    CacheHelper.Cache.Remove(cacheKey);
                    return true;                    
                }


            }
            return false;
            
        }

        public bool SubmitBase64PNG(Guid id, string data, string name)
        {

            if (string.IsNullOrWhiteSpace(data))
                return false;
            byte[] f = System.Convert.FromBase64String(data);
            //MemoryStream ms = new MemoryStream(data);
            //Image img = Image.FromStream(ms);
            var contact = _users.ContactID;
            var company = _users.DefaultContactCompanyID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {

                var d = new NKDC(_users.ApplicationConnectionString, null);
                if (contact.HasValue)
                {
                    var m = (from o in d.Contacts where o.ContactID == contact && o.Version == 0 && o.VersionDeletedBy == null select o).Single();
                    var file = new FileData
                    {
                        FileDataID = id,
                        TableType = null,
                        ReferenceID = contact,
                        FileTypeID = null, //TODO give type
                        FileName = name,
                        FileLength = f.Length,
                        MimeType = "image/png",
                        VersionOwnerContactID = contact,
                        VersionOwnerCompanyID = company,
                        VersionUpdated = DateTime.UtcNow,
                        VersionAntecedentID = id,
                        VersionUpdatedBy = contact,
                        DocumentType = ConstantsHelper.DOCUMENT_TYPE_BASE64PNG
                    };
                    file.FileChecksum = file.FileBytes.ComputeHash();
                    d.FileDatas.AddObject(file);
                    d.SaveChanges();
                    var cacheKey = string.Format("{0}-img-{1}", id, CacheHelper.CacheType.Preview);
                    CacheHelper.Cache.Remove(cacheKey);
                    return true;
                }


            }
            return false;

        }

        public bool DuplicateCompany(string companyName)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null, false);
                return d.Companies.Where(f => f.Version == 0 && f.VersionDeletedBy == null && f.CompanyName == companyName || f.CompanyName == (companyName ?? "") + "*").Any();
            }
        }
      
       
    }
}
