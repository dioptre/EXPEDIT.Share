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
        [HiddenInput, Required, DisplayName("SKU:")]
        public Guid? SupplierModelID { get; set; }
        public Guid? SupplierPartID { get; set; }
        public Guid? ModelID { get; set; } //ProductID
        public Guid? PartID { get; set; } //Also...ProductID
        public Guid? CompanyID { get; set; }
        public Guid? SupplierID { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Manufacturer { get; set; }
        public string UrlInternal { get; set; }
        public string UrlExternal { get; set; }
        public decimal? Rating { get; set; }
        public decimal? RatingScale { get; set; }
        public string HTML { get; set; }
        public string Thumbnail { get { return string.Format(@"{0}/Companies/{1}.png", MediaDirectory, CompanyID); } }
        public decimal? PricePerUnit { get; set; }
        public Guid? CurrencyID { get; set; }
        public string CurrencyPrefix { get; set; }
        public string CurrencyPostfix { get; set; }
        public Guid? PriceUnitID { get; set; }
        public string CostUnit { get; set; }        
        public Guid? FreeDownloadID { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? Downloads { get; set; }
        public string MediaDirectory { get; set; }
        public Guid? PaymentProviderID { get; set; }
        public Guid? PaymentProviderProductID { get; set; }
        public string PaymentProviderProductName { get; set; }
        public Guid? ProductUnitID { get; set; }
        public string ProductUnitName { get; set; }
        public string ProductUnitNamePaymentProvider { get; set; }
        public bool? IsRecurring { get; set; }
        public Guid? ProductID { get; set; } //ModelID or PartID
        public decimal? KitUnitDefault { get; set; }
        public decimal? KitUnitMaximum { get; set; }
        public decimal? KitUnitMinimum { get; set; }
        public decimal? UnitDefault { get; set; }
        public decimal? UnitMaximum { get; set; }
        public decimal? UnitMinimum { get; set; }
         

    }
}