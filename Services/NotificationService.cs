using System;
using System.Linq;
using NKD.Module.BusinessObjects;

using PushSharp;
using PushSharp.Android;
using System.Threading;
using System.Transactions;
using NKD.Services;
using Orchard.Logging;
using Orchard.Localization;
using JetBrains.Annotations;
using Orchard;
using Orchard.Environment;

namespace EXPEDIT.Share.Services
{

    [UsedImplicitly]
    public class NotificationService : INotificationService, IOrchardShellEvents
    {
        private static PushBroker _pushBroker = new PushBroker();
        public static PushBroker PushBroker { get { return _pushBroker; } }

        private readonly IUsersService _users;
        public ILogger Logger { get; set; }
        private readonly IOrchardServices _services;

        public NotificationService(
            IUsersService users,
            IOrchardServices orchardServices
            )
        {
            _users = users;
            _services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }

        public void Activated() {

        }

        public void Terminating()
        {
            _pushBroker.StopAllServices();
        }


        public bool RegisterDevice(string deviceType, string id, int? timezone)
        {
            switch (deviceType)
            {
                case EXPEDIT.Share.Helpers.ConstantsHelper.NOTIFICATION_APPLE:
                    break;
                case EXPEDIT.Share.Helpers.ConstantsHelper.NOTIFICATION_ANDROID:
                    break;
                default:
                    return false;
            }
            var contact = _users.ContactID;
            if (!contact.HasValue)
                return false;
            var company = _users.ApplicationCompanyID;
            var now = DateTime.UtcNow;
            var isNew = false;
            Notification notification = null;
            string tableType = null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                notification = (from o in d.Notifications where o.ContactID == contact select o).FirstOrDefault();
                if (notification == null)
                {
                    notification = new Notification
                    {
                        ContactID = contact,
                        NotificationID = Guid.NewGuid(),
                        Timezone = timezone,
                        VersionOwnerContactID = contact,
                        VersionUpdated = now,
                        VersionUpdatedBy = contact
                    };
                    d.Notifications.AddObject(notification);
                    notification.NotificationDevices.Add(new NotificationDevice
                    {
                        DeviceToken = id,
                        DeviceType = deviceType,
                        NotificationID = notification.NotificationID,
                        LastRegistered = now,
                        VersionOwnerContactID = contact,
                        VersionUpdated = now,
                        VersionUpdatedBy = contact
                    });
                    isNew = true;
                }
                else
                {
                    var devices = (from o in notification.NotificationDevices where o.DeviceToken == id && o.DeviceType == deviceType && o.Version == 0 && o.VersionDeletedBy == null select o);
                    if (devices.Any())
                    {
                        foreach (var o in devices)
                            o.LastRegistered = now;
                    }
                    else
                    {
                        notification.NotificationDevices.Add(new NotificationDevice
                        {
                            DeviceToken = id,
                            DeviceType = deviceType,
                            NotificationID = notification.NotificationID,
                            LastRegistered = now,
                            VersionOwnerContactID = contact,
                            VersionUpdated = now,
                            VersionUpdatedBy = contact
                        });
                        isNew = true;
                    }

                }
                d.SaveChanges();
                if (isNew)
                    tableType = d.GetTableName(typeof(Notification));
            }
            if (isNew)
            {
                var jsJoined = string.Format("{{ \"message\":\"Welcome to {0}.\", \"title\": \"You added a device to {0}.\", \"msgcnt\" : \"1\" }}", _services.WorkContext.CurrentSite.SiteName, Guid.NewGuid());
                SendNotification(new Guid[] {contact.Value}, jsJoined, tableType, notification.NotificationID);
            }
            return true;

        }

        public bool SendNotification(Guid[] contacts, string json, string tableType = null, Guid? referenceID = null)
        {
            if (contacts == null || contacts.Length == 0)
                return false;
            else 
                contacts = contacts.Distinct().ToArray();
            bool succeeded = false;
            NotificationDevice[] devices = new NotificationDevice[0];
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var deviceExpiry = DateTime.UtcNow.AddDays(-365);
                devices = (from o in d.NotificationDevices
                           where
                               (!o.Notification.ContactID.HasValue || contacts.Contains(o.Notification.ContactID.Value)) &&
                               o.LastRegistered > deviceExpiry &&
                               o.Notification.Version == 0 &&
                               o.Notification.VersionDeletedBy == null &&
                               o.Version == 0 &&
                               o.VersionDeletedBy == null
                           select o).ToArray();
                if (devices.Length == 0)
                    return false;
                if (devices.Length > 0 && devices[0].NotificationID.HasValue)
                {
                    var history = new NotificationData
                    {
                        NotificationID = devices[0].NotificationID.Value,
                        JSON = json,
                        TableType = tableType,
                        ReferenceID = referenceID
                    };
                    d.NotificationDatas.AddObject(history);
                    d.SaveChanges();
                }
            }
            PushSharp.Core.IPushService aSvc = null;
            if (devices.Any(f => f.DeviceType == EXPEDIT.Share.Helpers.ConstantsHelper.NOTIFICATION_ANDROID))
            {
                aSvc = PushBroker.GetRegistrations<GcmNotification>().FirstOrDefault();
                if (aSvc == null)
                {
                    PushBroker.RegisterGcmService(new GcmPushChannelSettings(EXPEDIT.Share.Helpers.ConstantsHelper.GcmAuth));
                    aSvc = PushBroker.GetRegistrations<GcmNotification>().First();
                }
            }
            var ewhs = new EventWaitHandle[devices.Length];
            var sent = new PushSharp.Core.NotificationSentDelegate[devices.Length];
            var failed = new PushSharp.Core.NotificationFailedDelegate[devices.Length];
            for (var i = 0; i < devices.Length; i++)
            {
                ewhs[i] = new EventWaitHandle(false, EventResetMode.ManualReset);
                var ewh = ewhs[i];
                switch (devices[i].DeviceType)
                {
                    case EXPEDIT.Share.Helpers.ConstantsHelper.NOTIFICATION_ANDROID:
                        var notification = new GcmNotification()
                                .ForDeviceRegistrationId(devices[i].DeviceToken)
                                .WithJson(json)
                                .WithDelayWhileIdle(false);

                        PushSharp.Core.NotificationSentDelegate msgSent = (object sender, PushSharp.Core.INotification msg) =>
                        {
                            if (msg.QueuedCount == notification.QueuedCount)
                            {
                                succeeded = true;
                                ewh.Set();
                            }
                        };
                        PushSharp.Core.NotificationFailedDelegate msgFailed = (object sender, PushSharp.Core.INotification msg, Exception ex) =>
                        {
                            if (msg.QueuedCount == notification.QueuedCount)
                            {
                                ewh.Set();
                            }
                        };
                        aSvc.OnNotificationSent += msgSent;
                        aSvc.OnNotificationFailed += msgFailed;
                        PushBroker.QueueNotification(notification);
                        break;
                    case EXPEDIT.Share.Helpers.ConstantsHelper.NOTIFICATION_APPLE:
                        //var appleCert = File.ReadAllBytes("ApnsSandboxCert.p12"));
                        //push.RegisterAppleService(new ApplePushChannelSettings(appleCert, "pwd"));
                        //push.QueueNotification(new AppleNotification()
                        //       .ForDeviceToken("DEVICE TOKEN HERE")
                        //       .WithAlert("Hello World!")
                        //       .WithBadge(7)
                        //       .WithSound("sound.caf"));
                        break;
                }

            }

            WaitHandle.WaitAll(ewhs, 40000); //Wait only 40secs 
            for (var i = 0; i < devices.Length; i++)
            {
                aSvc.OnNotificationSent -= sent[i];
                aSvc.OnNotificationFailed -= failed[i];
            }
            return succeeded; //any msg got through
        }

        public bool Chat(string username, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;
            var contact = _users.ContactID;
            if (!contact.HasValue)
                return false;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var recipientID = (from o in d.Contacts where o.Username == username select o.ContactID).First();
                if (message.Length > 255)
                    message = message.Substring(0, 255);
                message = message.Replace("\"", "\\\"");
                var js = string.Format("{{ \"message\":\"{1}\", \"title\": \"{0} message from {1}.\", \"msgcnt\" : \"1\" }}", _services.WorkContext.CurrentSite.SiteName, _services.WorkContext.CurrentUser.UserName, message);
                return SendNotification(new Guid[] {recipientID}, js, d.GetTableName(typeof(Contact)), contact.Value);
            }
        }
    }
}