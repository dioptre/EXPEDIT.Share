using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using System.ServiceModel;
using NKD.Module.BusinessObjects;
using System.Web.Mvc;
namespace EXPEDIT.Share.Services
{
     [ServiceContract]
    public interface IContentService : IDependency 
    {

         [OperationContract]
         SelectListItem[] GetCountries(string startsWith);


         [OperationContract]
         SelectListItem[] GetLocations(string startsWith);

         [OperationContract]
         Affiliate UpdateAffiliate(Guid? childAffiliateID = default(Guid?), Guid? parentAffiliateID = default(Guid?), string requestIPAddress = null, bool referral=false, bool checkin=false, string reference=null);

         [OperationContract]
         int GetAffiliateCount();

         [OperationContract]
         int GetAffiliatePoints();


    }
}