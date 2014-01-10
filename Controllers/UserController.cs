using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using EXPEDIT.Share.Services;
using Orchard.Themes;
using XODB.Helpers;
using System;
using EXPEDIT.Share.Helpers;
using Orchard.DisplayManagement;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Search.Models;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Indexing;
using Orchard.Logging;
using Orchard.Collections;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;


namespace EXPEDIT.Share.Controllers {
    
    [Themed]
    public class UserController : Controller {
        public IOrchardServices Services { get; set; }
        private IShareService _share { get; set; }
        private IContentService _content { get; set; }
        
        public ILogger Logger { get; set; }
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public UserController(
            IOrchardServices services, 
            IShareService share, 
            IContentService content,
            ISearchService searchService,
            IContentManager contentManager,
            ISiteService siteService,
            IShapeFactory shapeFactory
            ) {
            _share = share;
            Services = services;
            _content = content;
            T = NullLocalizer.Instance;

            _searchService = searchService;
            _contentManager = contentManager;
            _siteService = siteService;
            Shape = shapeFactory;
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


        dynamic Shape { get; set; }
        [Themed(true)]
        public ActionResult Search(PagerParameters pagerParameters, string q = "")
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchFields = Services.WorkContext.CurrentSite.As<SearchSettingsPart>().SearchedFields;

            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try
            {
                searchHits = _searchService.Query(q, pager.Page, pager.PageSize,
                                                  Services.WorkContext.CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                                                  searchFields,
                                                  searchHit => searchHit);
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Invalid search query: {0}", exception.Message));
            }

            var list = Shape.List();
            var offset = (pager.Page - 1) * pager.PageSize + 1;
            var dbSearch = _share.GetSearchResults(q, null, offset, pager.PageSize);
            var dbCount = dbSearch.Count();
            foreach (var hit in dbSearch)
            {
                list.Add(hit);
            }

            var foundIds = searchHits.Select(searchHit => searchHit.ContentItemId).ToList();

            // ignore search results which content item has been removed or unpublished
            var foundItems = _contentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints()).ToList();
            foreach (var contentItem in foundItems)
            {
                list.Add(_contentManager.BuildDisplay(contentItem, "Summary"));                
            }
            searchHits.TotalItemCount -= foundIds.Count() - foundItems.Count();

            var pagerShape = Shape.Pager(pager).TotalItemCount(searchHits.TotalItemCount);

            var searchViewModel = new SearchViewModel
            {
                Query = q,
                TotalItemCount = searchHits.TotalItemCount + dbCount,
                StartPosition = offset,
                EndPosition = pager.Page * pager.PageSize > (searchHits.TotalItemCount + dbCount) ? (searchHits.TotalItemCount + dbCount) : pager.Page * pager.PageSize, //TODO: Hack, fix
                ContentItems = list,
                Pager = pagerShape
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }


    }
}
