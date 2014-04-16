using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EXPEDIT.Share.ViewModels
{
    public class SearchViewModel
    {
        public string id { get { return string.Format("{0}",ReferenceID); } }
        public long? Sequence { get; set; }
        public long? Total { get; set; }
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UrlInternal { get; set; }
    }
}