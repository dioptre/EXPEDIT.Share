using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EXPEDIT.Share.ViewModels
{
    public class PickLocationViewModel
    {
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }
        public Guid? LocationID { get; set; }
        [DisplayName("Location Name")]
        public string LocationName { get; set; }
        public string Geography { get; set; }
        public string Comment { get; set; }
        [DisplayName("Country")]
        public string CountryID { get; set; }
        public Guid? CountryStateID { get; set; }
        public string CountryStateName { get; set; }
        public string LocationCode { get; set; }
        [DisplayName("Post Code")]
        public string PostCode { get; set; }
        
        public string Query { get; set; }
        public IEnumerable<SearchViewModel> SearchResults { get; set; }
    }
}