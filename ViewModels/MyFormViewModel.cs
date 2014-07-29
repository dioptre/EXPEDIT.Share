using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;
using Newtonsoft.Json;

namespace EXPEDIT.Share.ViewModels
{
    public class MyFormViewModel 
    {
        public Guid? id { get { return FormID; } set { FormID = value; } }
        public string emails { get { return Recipients; } set { Recipients = value; } }
        public string fields { get; set; }
        [JsonIgnore]
        public dynamic Form { get; set; }

        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }       
        public string FormData { get; set; }
        public Guid? FormID { get; set; }
        public Guid? FormName { get; set; }
        public Guid? FormDataID { get; set; }
        public string FormJSON { get; set; }
        public string FormHash { get; set; }
        public string Recipients { get; set; } //Emails
        public HttpPostedFileBase[] Files { get; set; }
        public string FormOrigin { get; set; }
        public DateTime? Updated { get; set; }

    }
}