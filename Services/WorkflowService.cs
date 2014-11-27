using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard;
using System.Security.Principal;
using EXPEDIT.Share.ViewModels;


using System.Text;
using System.Security.Cryptography;
using System.Transactions;
using NKD.Module.BusinessObjects;
using NKD.Services;
using NKD.Models;
using NKD.Helpers;
using EXPEDIT.Share.Helpers;

namespace EXPEDIT.Share.Services {

    [UsedImplicitly]
    public class WorkflowService : IWorkflowService
    {
        private readonly IOrchardServices _services;
        private readonly IUsersService _users;

        public WorkflowService(
            IOrchardServices orchardServices,
            IUsersService users
            )
        {
            _users = users;
            _services = orchardServices;

        }

        public bool CreateInstance(WorkflowInstance m) { return false; }

        public bool CheckoutInstance(WorkflowInstance m, bool forced = false) { return false; }

        public bool CheckinInstance(WorkflowInstance m, bool forced = false) { return false; }

        public bool CancelInstance(WorkflowInstance m, bool forced = false) { return false; }

        public bool CompleteInstance(WorkflowInstance m, bool forced = false) { return false; }

        public bool CheckinIdleInstances() { return false; }

        public bool ExecutePendingInstances() { return false; }


    }
}
