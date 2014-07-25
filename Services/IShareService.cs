﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using NKD.Module.BusinessObjects;
using EXPEDIT.Share.ViewModels;
using NKD.ViewModels;
using NKD.Helpers;

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
         FileData GetPreview(Guid fileDataID, int width = 200, int? height = 200, bool crop = false, ImageHelper.ImageFormat format = ImageHelper.ImageFormat.jpeg);

         [OperationContract]
         byte[] GetPhoto(Guid userID);

         [OperationContract]
         IEnumerable<IHtmlString> GetSearchResults(string text = null, Guid? supplierModelID = null, int? startRowIndex = null, int? pageSize = null, string filterCategories = null);

         [OperationContract]
         IEnumerable<SearchViewModel> GetFiles(string text = null, int? startRowIndex = null, int? pageSize = null);

         [OperationContract]
         bool SubmitFiles(Dictionary<Guid, HttpPostedFileBase> files, Dictionary<Guid, int> fileLengths);

         [OperationContract]
         IEnumerable<SearchViewModel> GetLocations(string text = null, int? startRowIndex = null, int? pageSize = null);

         [OperationContract]
         bool SubmitLocation(Guid locationID, string locationName, string geography, string culture="en-US");

         [OperationContract]
         bool SubmitLocation(PickLocationViewModel m);

         [OperationContract]
         bool SubmitForm(MyFormViewModel m);

         [OperationContract]
         PickLocationViewModel GetLocation(Guid locationID);

         [OperationContract]
         ContactViewModel GetMyInfo();

         [OperationContract]
         bool SubmitPhoto(HttpPostedFileBase file);

    }
}