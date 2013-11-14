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
#if XODB
using XODB.Module.BusinessObjects;
#else
using EXPEDIT.Utils.DAL.Models;
#endif
using XODB.Services;
using Orchard.Media.Services;

namespace EXPEDIT.Share.Services {
    
    [UsedImplicitly]
    public class ShareService : IShareService {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IUsersService _users;
        private readonly IMediaService _media;
        public ILogger Logger { get; set; }

        public ShareService(IContentManager contentManager, IOrchardServices orchardServices, IMessageManager messageManager, IScheduledTaskManager taskManager, IUsersService users, IMediaService media)
        {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _messageManager = messageManager;
            _taskManager = taskManager;
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
                var statName = "Routes";
                var application = _users.ApplicationID;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new XODBC(_users.ApplicationConnectionString, null, false);
                    var route = (from o in d.ApplicationRoutes where o.ApplicationID == application && o.RouteURL == routeURL orderby o.Sequence descending select o).FirstOrDefault();
                    var routeTable = d.GetTableName(route.GetType());
                    if (route.IsCapturingStatistic.HasValue && route.IsCapturingStatistic.Value)
                    {
                        var stat = (from o in d.StatisticDatas
                                    where o.ReferenceID == route.ApplicationRouteID && o.TableType == routeTable
                                    && o.StatisticDataName == statName
                                    select o).FirstOrDefault();
                        if (stat == null)
                        {
                            stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = routeTable, ReferenceID = route.ApplicationRouteID, StatisticDataName = statName, Count = 0 };
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
                var statName = "Downloads";
                var application = _users.ApplicationID;
                var contact = _users.ContactID;                
                var company = _users.CompanyID;
                var server = _users.ServerID;                
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new XODBC(_users.ApplicationConnectionString, null, false);
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
                                && o.StatisticDataName == statName
                                select o).FirstOrDefault();
                    if (stat == null)
                    {
                        stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = downloadTable, ReferenceID = download.DownloadID, StatisticDataName = statName, Count = 0 };
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


        
       
    }
}
