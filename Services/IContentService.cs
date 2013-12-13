using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using XODB.Module.BusinessObjects;

namespace EXPEDIT.Share.Services
{
     [ServiceContract]
    public interface IContentService : IDependency 
    {
         [OperationContract]
         FileData GetInvoice(Guid invoiceID, string requestIPAddress);

    }
}