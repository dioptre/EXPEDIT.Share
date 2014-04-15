using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using NKD.Module.BusinessObjects;
using EXPEDIT.Share.ViewModels;

namespace EXPEDIT.Share.Services
{
     [ServiceContract]
    public interface IShareService : IDependency 
    {
         [OperationContract]
         string GetRedirect(string routeURL);

         [OperationContract]
         FileData GetDownload(string downloadID, string requestIPAddress);

         [OperationContract]
         FileData GetFile(Guid fileDataID);

         [OperationContract]
         FileData GetPreview(Guid fileDataID);

         [OperationContract]
         IEnumerable<IHtmlString> GetSearchResults(string text = null, Guid? supplierModelID = null, int? startRowIndex = null, int? pageSize = null);

         [OperationContract]
         IEnumerable<SearchViewModel> GetFiles(string text = null, int? startRowIndex = null, int? pageSize = null);

         [OperationContract]
         bool SubmitFiles(Dictionary<Guid, HttpPostedFileBase> files, Dictionary<Guid, int> fileLengths);

    }
}