using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using XODB.Module.BusinessObjects;
using System.Web.Mvc;
namespace EXPEDIT.Share.Services
{
     [ServiceContract]
    public interface IContentService : IDependency 
    {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="invoiceID"></param>
         /// <param name="requestIPAddress"></param>
         /// <returns>DownloadID</returns>
         [OperationContract]
         Guid? GetInvoice(Guid invoiceID, string requestIPAddress);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="invoiceID"></param>
         /// <param name="requestIPAddress"></param>
         /// <returns>DownloadID</returns>
         [OperationContract]
         Guid? GetOrderInvoice(Guid orderID, string requestIPAddress);

         [OperationContract]
         SelectListItem[] GetCountries(string startsWith);
    }
}