using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.ViewModels;

namespace EXPEDIT.Share.ViewModels
{
    public class PickFileViewModel : IFileData
    {
        public Guid? ReferenceID { get; set; }
        public string TableType { get; set; }
        public Guid FileDataID { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public byte[] FileBytes { get; set; }

        public string Query { get; set; }
        public Dictionary<Guid, int> QueryFileLengths { get; set; }
        public Dictionary<Guid, HttpPostedFileBase> QueryFiles { get; set; }
        public IEnumerable<SearchViewModel> SearchResults { get; set; }
    }
}