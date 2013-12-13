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
using EXPEDIT.Share.Helpers;

namespace EXPEDIT.Share.Services {
    
    [UsedImplicitly]
    public class ContentService : IContentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IUsersService _users;
        private readonly IMediaService _media;
        public ILogger Logger { get; set; }

        public ContentService(IContentManager contentManager, IOrchardServices orchardServices, IMessageManager messageManager, IScheduledTaskManager taskManager, IUsersService users, IMediaService media)
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

        public Guid? GetInvoice(Guid invoiceID, string requestIPAddress)
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
                    var invoice = (from o in d.Invoices where o.InvoiceID == invoiceID select o).Single();
                    if (contact != invoice.CustomerContactID) //TODO: check other contact owner/company etc
                        throw new OrchardSecurityException(T("Not authorized to view invoice"));
                    var download = (from o in d.Downloads join f in d.FileDatas on o.FileDataID equals f.FileDataID where f.ReferenceID == invoiceID && f.TableType == d.GetTableName(typeof(Invoice), true) select o).FirstOrDefault();
                    if (download != null)
                        return download.DownloadID;
                    var file = (from o in d.FileDatas select o);
                    Stream stream = new MemoryStream();
                    PdfHelper.Html2Pdf("<h1>hi</h1>", ref stream);
                    throw new NotImplementedException();
                   
                }
            }
            catch
            {
                return null;
            }
        }


        
       
    }
}
