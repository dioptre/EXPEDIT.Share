using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using EXPEDIT.Share.Services;
using Orchard.Themes;
using XODB.Helpers;
using System;
using EXPEDIT.Share.Helpers;

namespace EXPEDIT.Share.Controllers {
    
    [Themed]
    public class UserController : Controller {
        public IOrchardServices Services { get; set; }
        private IShareService _share { get; set; }
        private IContentService _content { get; set; }

        public UserController(IOrchardServices services, IShareService share, IContentService content) {
            _share = share;
            Services = services;
            _content = content;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Redirect/Route Utility
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        //[Themed(false)]
        public ActionResult Go(string id) 
        {
            var redirect = _share.GetRedirect(id);
            if (!string.IsNullOrWhiteSpace(redirect))
                Response.Redirect(redirect);
            return new HttpNotFoundResult();
        }

        /// <summary>
        /// Download a file (can use a sqlfilestream eventually) TODO:
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="contactid"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        //[Themed(false)]
        [Authorize]
        public ActionResult Download(string id)
        {

            var file = _share.GetDownload(id, Request.GetIPAddress());
            if (file != null)
                return new XODB.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}", id, XODB.Helpers.DateHelper.NowInOnlineFormat, file.FileName).Trim(), "application/octet", stream => new System.IO.MemoryStream(file.FileBytes).WriteTo(stream));
            return new HttpNotFoundResult();
        }


        [ValidateInput(false)]
        [Authorize]
        //[ValidateAntiForgeryToken]
        [Themed(true)]
        public ActionResult GetInvoice(string id)
        {
            return Download(string.Format("{0}",_content.GetInvoice(new Guid(id), Request.GetIPAddress())));
        }

        [ValidateInput(false)]
        [Authorize]
        //[ValidateAntiForgeryToken]
        [Themed(true)]
        public ActionResult GetOrderInvoice(string id)
        {
            return Download(string.Format("{0}", _content.GetOrderInvoice(new Guid(id), Request.GetIPAddress())));
        }

        [ValidateInput(false)]
        [Authorize]
        [Themed(false)]
        public JsonResult GetCountries(string id)
        {                       
            return Json(_content.GetCountries(id), JsonRequestBehavior.AllowGet);
        }

    }
}
