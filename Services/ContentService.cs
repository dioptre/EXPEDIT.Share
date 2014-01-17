﻿using System;
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
using System.Web.Mvc;
using XODB.Helpers;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace EXPEDIT.Share.Services
{

    [UsedImplicitly]
    public class ContentService : IContentService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IUsersService _users;
        private readonly IMediaService _media;
        private readonly IStorageProvider _storage;
        public ILogger Logger { get; set; }

        public ContentService(
            IContentManager contentManager,
            IOrchardServices orchardServices,
            IMessageManager messageManager,
            IScheduledTaskManager taskManager,
            IUsersService users,
            IMediaService media,
            IStorageProvider storage)
        {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _messageManager = messageManager;
            _taskManager = taskManager;
            _media = media;
            _users = users;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _storage = storage;
        }

        public Localizer T { get; set; }

        public Guid? GetOrderInvoice(Guid orderID, string requestIPAddress)
        {
            try
            {
                var contact = _users.ContactID;
                var invoiceID = default(Guid?);
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new XODBC(_users.ApplicationConnectionString, null, false);
                    invoiceID = (from o in d.Supplies
                                 join i in d.Invoices on o.SupplyID equals i.SupplyID
                                 where o.CustomerPurchaseOrderID == orderID && o.PurchaseOrderCustomer.CustomerContactID == contact
                                 && o.Version == 0 && o.VersionDeletedBy == null
                                 && i.Version == 0 && i.VersionDeletedBy == null
                                 orderby i.VersionUpdated descending
                                 select i.InvoiceID).FirstOrDefault();
                }
                if (invoiceID.HasValue)
                    return GetInvoice(invoiceID.Value, requestIPAddress);
                return null;
            }
            catch
            {
                return null;
            }
        }

        public Guid? GetInvoice(Guid invoiceID, string requestIPAddress)
        {
            try
            {
                var application = _users.ApplicationID;
                var contact = _users.ContactID;
                var company = _users.ApplicationCompanyID;
                var server = _users.ServerID;
                var now = DateTime.UtcNow;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new XODBC(_users.ApplicationConnectionString, null, false);
                    var invoiceTableType = d.GetTableName(typeof(Invoice), true);
                    var invoice = (from o in d.Invoices
                                   where o.InvoiceID == invoiceID
                                   && o.Version == 0 && o.VersionDeletedBy == null
                                   select o).Single();
                    if (contact != invoice.CustomerContactID) //TODO: check other contact owner/company etc
                        throw new OrchardSecurityException(T(string.Format("Not authorized to view invoice contact: {0} invoice: {1}", contact, invoiceID)));
                    var download = (from o in d.Downloads
                                    join f in d.FileDatas on o.FileDataID equals f.FileDataID
                                    where f.ReferenceID == invoiceID && f.TableType == invoiceTableType
                                    && f.VersionDeletedBy == null && f.Version == 0 && o.VersionDeletedBy == null && o.Version == 0
                                    select o).FirstOrDefault();
                    if (download != null)
                        return download.DownloadID;
                    Stream stream = new MemoryStream();
                    invoice.GetPDF(ref stream, _storage.GetAbsolutePath(ConstantsHelper.PDF_LOGO));
                    stream.Seek(0, SeekOrigin.Begin);
                    var bytes = stream.ToByteArray();
                    var file = new FileData
                    {
                        FileDataID = Guid.NewGuid()
                        ,
                        TableType = invoiceTableType
                        ,
                        ReferenceID = invoiceID
                        ,
                        FileBytes = bytes
                        ,
                        FileTypeID = ConstantsHelper.FILE_TYPE_INVOICE
                        ,
                        FileName = string.Format("Invoice-[{0}].pdf", invoiceID)
                        ,
                        FileChecksum = bytes.ComputeHash()
                    };
                    stream.Close();
                    download = new Download
                    {
                        DownloadID = Guid.NewGuid(),
                        FileAllocated = now,
                        FilterContactID = contact,
                        RemainingDownloads = ConstantsHelper.DOWNLOADS_REMAINING_DEFAULT,
                        FileDataID = file.FileDataID,
                        ValidFrom = now,
                    };
                    download.FileData = file;
                    d.Downloads.AddObject(download);
                    d.SaveChanges();
                    return download.DownloadID;
                }
            }
            catch
            {
                return null;
            }
        }

        public SelectListItem[] GetCountries(string startsWith)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new XODBC(_users.ApplicationConnectionString, null, false);
                d.ContextOptions.LazyLoadingEnabled = false;
                return (from o in d.DictionaryCountries
                        where o.StandardCountryName.StartsWith(startsWith)
                        select new SelectListItem { Text = o.StandardCountryName, Value = o.CountryID }).ToArray();
            }
        }


        public Guid? GetAffiliateID(string requestIPAddress)
        {
            var contact = _users.ContactID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new XODBC(_users.ApplicationConnectionString, null, false);
                Affiliate a = null;
                if (contact.HasValue)
                    a = (from o in d.Affiliates where o.AffiliateContactID == contact && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                else if (!string.IsNullOrWhiteSpace(requestIPAddress))
                    a = (from o in d.Affiliates where o.InitialIP == requestIPAddress && o.AffiliateContactID == null && o.Version == 0 && o.VersionDeletedBy == null orderby o.VersionUpdated ascending select o).FirstOrDefault();
                if (a == null && (contact.HasValue || !string.IsNullOrWhiteSpace(requestIPAddress))) //Create new ID
                {
                    a = new Affiliate
                    {
                        AffiliateID = Guid.NewGuid(),
                        AffiliateContactID = contact,
                        InitialIP = requestIPAddress
                    };
                    d.Affiliates.AddObject(a);
                    d.SaveChanges();
                }
                if (a == null)
                    return null;
                return a.AffiliateID;
            }

        }


        public void UpdateAffiliateChild(Guid parentAffiliateID, Guid childAffiliateID)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new XODBC(_users.ApplicationConnectionString, null, false);
                var parent = (from o in d.Affiliates where o.AffiliateID == parentAffiliateID && o.AffiliateContactID != null && o.Version == 0 && o.VersionDeletedBy == null select o.AffiliateContactID).FirstOrDefault();
                if (!parent.HasValue)
                    return;
                var a = (from o in d.Affiliates where o.AffiliateID == childAffiliateID && o.ParentContactID == null && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                if (a != null)
                {
                    a.ParentContactID = parent.Value;
                    d.SaveChanges();
                }
            }

        }

        public void UpdateAffiliateRequest(Guid parentAffiliateID, string requestIPAddress)
        {
            var child = GetAffiliateID(requestIPAddress);
            if (child.HasValue)
                UpdateAffiliateChild(parentAffiliateID, child.Value);
        }

    }
}
