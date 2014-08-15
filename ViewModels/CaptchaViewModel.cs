using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;
using Newtonsoft.Json;

namespace EXPEDIT.Share.ViewModels
{
    public class CaptchaViewModel 
    {
        public string Image64 { get; set; }
        public Guid? MetaDataID { get; set; }
        public string Key { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Expiry { get; set; }
        public bool? IsValid { get; set; }
        public Guid? Cookie { get { return MetaDataID; } set { MetaDataID = value; } }

    }
}