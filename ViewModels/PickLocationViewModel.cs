using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;

namespace EXPEDIT.Share.ViewModels
{
    public class PickLocationViewModel
    {
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }
        public Guid LocationID { get; set; }
        public string LocationName { get; set; }
        public string Geography { get; set; }

        public string Query { get; set; }
        public IEnumerable<SearchViewModel> SearchResults { get; set; }
    }
}