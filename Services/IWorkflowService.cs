using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using NKD.ViewModels;
using EXPEDIT.Share.ViewModels;
using System.Data;
using NKD.Models;
using NKD.Module.BusinessObjects;

namespace EXPEDIT.Share.Services
{
    [ServiceContract]
    public interface IWorkflowService : IDependency
    {

        [OperationContract]
        bool CreateInstance(WorkflowInstance m);
        [OperationContract]
        bool CheckoutInstance(WorkflowInstance m, bool forced = false);
        [OperationContract]
        bool CheckinInstance(WorkflowInstance m, bool forced = false);
        [OperationContract]
        bool CancelInstance(WorkflowInstance m, bool forced = false);
        [OperationContract]
        bool CompleteInstance(WorkflowInstance m, bool forced = false);
        [OperationContract]
        bool CheckinIdleInstances();
        [OperationContract]
        bool ExecutePendingInstances();



    }
}