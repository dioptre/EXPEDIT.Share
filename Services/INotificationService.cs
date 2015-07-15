using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using NKD.ViewModels;
using System.Data;
using NKD.Models;
using System.Web.Mvc;

namespace EXPEDIT.Share.Services
{
    [ServiceContract]
    public interface INotificationService : IDependency
    {
       
        [OperationContract]
        bool RegisterDevice(string deviceType, string id, int? timezone);

        [OperationContract]
        bool SendNotification(Guid[] contacts, string json, string tableType = null, Guid? referenceID = null);

        [OperationContract]
        bool Chat(string username, string message);
    }
}