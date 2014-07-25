using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;

namespace EXPEDIT.Share.ViewModels
{
    public class MyFormViewModel 
    {
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }       
        public Guid? id { get { return FormID; } set { FormID = value; } }
        public Guid? FormID { get; set; }
        public Guid? FormName { get; set; }
        public Guid? FormDataID { get; set; }
        public string Json { get; set; }
        public string Fields { get; set; }
        public string Recipients { get; set; } //Emails
        public HttpPostedFileBase[] Files { get; set; }

    }
}