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
using EXPEDIT.Share.Helpers;
using System.Web.Mvc;
using NKD.Helpers;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Orchard.Users.Events;


namespace EXPEDIT.Share.Services
{

    [UsedImplicitly]
    public class ContentService : IContentService, Orchard.Users.Events.IUserEventHandler
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

        public SelectListItem[] GetCountries(string startsWith)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null, false);
                d.ContextOptions.LazyLoadingEnabled = false;
                return (from o in d.DictionaryCountries
                        where o.StandardCountryName.StartsWith(startsWith)
                        select new SelectListItem { Text = o.StandardCountryName, Value = o.CountryID }).ToArray();
            }
        }




        public Affiliate UpdateAffiliate(Guid? childAffiliateID = default(Guid?), Guid? parentAffiliateID = default(Guid?), string requestIPAddress = null, bool referral=false, bool checkin=false, string reference=null)
        {
            var contact = _users.ContactID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null, false);
                Guid? parent = null;
                if (parentAffiliateID.HasValue)
                    parent = (from o in d.Affiliates where o.AffiliateID == parentAffiliateID && o.AffiliateContactID != null && o.Version == 0 && o.VersionDeletedBy == null select o.AffiliateContactID).FirstOrDefault();
                Affiliate child = null;
                if (!childAffiliateID.HasValue)
                    child = (from o in d.Affiliates where o.AffiliateID == childAffiliateID && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                if (child == null)
                {
                    if (contact.HasValue)
                        child = (from o in d.Affiliates where o.AffiliateContactID == contact && o.Version == 0 && o.VersionDeletedBy == null select o).FirstOrDefault();
                    else if (!string.IsNullOrWhiteSpace(requestIPAddress))
                    {
                        child = (from o in d.Affiliates where o.InitialIP == requestIPAddress && o.AffiliateContactID == null && o.Version == 0 && o.VersionDeletedBy == null orderby o.VersionUpdated ascending select o).FirstOrDefault();
                        if (child == null)
                            child = (from o in d.Affiliates where o.InitialIP == requestIPAddress && o.Version == 0 && o.VersionDeletedBy == null orderby o.VersionUpdated ascending select o).FirstOrDefault();
                    }
                    if (child == null && (contact.HasValue || !string.IsNullOrWhiteSpace(requestIPAddress))) //Create new ID
                    {
                        child = new Affiliate
                        {
                            AffiliateID = Guid.NewGuid(),
                            AffiliateContactID = contact,
                            InitialIP = requestIPAddress
                        };
                        d.Affiliates.AddObject(child);
                    }
                }
                var table = d.GetTableName<Affiliate>();
                if (child != null)
                {
                    //Now try and update
                    if (string.IsNullOrWhiteSpace(child.InitialIP) && !string.IsNullOrWhiteSpace(requestIPAddress))
                        child.InitialIP = requestIPAddress;
                    if (parent.HasValue && !child.ParentContactID.HasValue)
                        child.ParentContactID = parent.Value;
                    if (referral && parentAffiliateID.HasValue)
                    {

                        var stat = (from o in d.StatisticDatas
                                    where o.ReferenceID == parentAffiliateID.Value && o.TableType == table
                                    && o.StatisticDataName == ConstantsHelper.STAT_NAME_REFERRAL
                                    select o).FirstOrDefault();
                        if (stat == null)
                        {
                            stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = table, ReferenceID = parentAffiliateID.Value, StatisticDataName = ConstantsHelper.STAT_NAME_REFERRAL, Count = 0 };
                            d.StatisticDatas.AddObject(stat);
                        }
                        stat.Count++;
                    }
                }
                if (checkin)
                {
                    StatisticData stat;
                    string externalRef;
                    bool userMissing = string.IsNullOrWhiteSpace(reference);
                    if (userMissing)
                        externalRef = requestIPAddress;
                    else
                        externalRef = string.Join("", string.Format("{0}+{1}", requestIPAddress, reference).Take(50));
                    if (child != null)
                        stat = (from o in d.StatisticDatas
                                where o.ReferenceID == child.AffiliateID && o.TableType == table
                                && o.StatisticDataName == ConstantsHelper.STAT_NAME_CHECKIN
                                select o).FirstOrDefault();
                    else //this should never happen
                        stat = (from o in d.StatisticDatas
                                where o.ReferenceExternal.StartsWith(requestIPAddress)
                                && o.StatisticDataName == ConstantsHelper.STAT_NAME_CHECKIN
                                select o).FirstOrDefault();
                    if (stat == null)
                    {
                        stat = new StatisticData { StatisticDataID = Guid.NewGuid(), TableType = table, ReferenceID = (child == null) ? default(Guid?) : child.AffiliateID, StatisticDataName = ConstantsHelper.STAT_NAME_CHECKIN, Count = 0 };
                        d.StatisticDatas.AddObject(stat);
                    }
                    stat.Count++;
                    if (string.IsNullOrWhiteSpace(stat.ReferenceExternal)) //Let's save what we can
                        stat.ReferenceExternal = externalRef;
                    else if (stat.ReferenceExternal != externalRef)
                    {
                        if (!userMissing)
                            stat.ReferenceExternal = externalRef;
                        else if (stat.ReferenceExternal.IndexOf('+') < 0)
                            stat.ReferenceExternal = externalRef;
                    }
                        
                }
                d.SaveChanges();
                return child;
            }

        }

    

        public void Creating(UserContext context) { }

        public void Created(UserContext context)
        {
            UpdateAffiliate();          
        }

        public void LoggedIn(IUser user)
        {
            UpdateAffiliate();    
        }

        public void LoggedOut(IUser user) { }

        public void AccessDenied(IUser user) { }

        public void ChangedPassword(IUser user) { }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user)
        {
            
        }

        public int GetAffiliateCount()
        {
            var contact = _users.ContactID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null, false);
                return (from o in d.Affiliates where o.ParentContactID==contact select o.ParentContactID).Count();
            }
        }

        public int GetAffiliatePoints()
        {
            return 0;
        }

    }
}
