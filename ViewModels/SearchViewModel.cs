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
        public long? Row { get; set; }
        public long? TotalRows { get; set; }
        public decimal? Score { get; set; }
        public Guid? id { get; set; }
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SpatialJSON { get; set; }
        public string InternalUrl { get; set; }
        public string ExternalUrl { get; set; }
        public string Author { get; set; }
        public DateTime? Updated { get; set; }
    }
}