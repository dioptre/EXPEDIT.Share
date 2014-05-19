using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using EXPEDIT.Share.Services;
using Orchard.Themes;
using NKD.Helpers;
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
using Orchard.Mvc;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Dynamic;
using ImpromptuInterface.Dynamic;
using System.Threading;
using System.Globalization;


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
            )
        {
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
                return new RedirectResult(redirect);
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
                return new NKD.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}", id, NKD.Helpers.DateHelper.NowInOnlineFormat, file.FileName).Trim(), "application/octet", stream => new System.IO.MemoryStream(file.FileBytes).WriteTo(stream));
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
        //[Authorize]
        public ActionResult File(string id)
        {
            var file = _share.GetFile(new Guid(id));
            if (file != null)
                return new NKD.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}", id, NKD.Helpers.DateHelper.NowInOnlineFormat, file.FileName).Trim(), "application/octet", stream => new System.IO.MemoryStream(file.FileBytes).WriteTo(stream));
            return new HttpNotFoundResult();
        }


        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="contactid"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        //[Themed(false)]
        //[Authorize]
        public ActionResult Preview(string id)
        {
            var file = _share.GetPreview(new Guid(id));
            if (file != null)
            {
                if (file.FileBytes != null)
                    return new NKD.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}.png", id, NKD.Helpers.DateHelper.NowInOnlineFormat, file.FileName).Trim(), "image/png", stream => new System.IO.MemoryStream(file.FileBytes).WriteTo(stream));
                else
                    return new RedirectResult(System.Web.VirtualPathUtility.ToAbsolute("~/Media/Default/EXPEDIT.Share/images/qmark.jpg"));                    
            }
            return new HttpNotFoundResult();
        }


        [ValidateInput(false)]
        //[Authorize]
        [Themed(false)]
        public JsonResult GetCountries(string id)
        {                       
            return Json(_content.GetCountries(id), JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        //[Authorize]
        [Themed(false)]
        public JsonResult GetLocations(string id)
        {
            return Json(_content.GetLocations(id), JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        [Authorize]
        [Themed(false)]
        public JsonResult GetUsernames(string id)
        {
            return Json(_content.GetUsernames(id), JsonRequestBehavior.AllowGet);
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
                searchHits = _searchService.Query(
                    q, 
                    pager.Page, 
                    pager.PageSize, 
                    false,                                                  
                    null,                                                                      
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

        [ValidateInput(false)]
        public ActionResult Refer(string id, string name)
        {
            try
            {
                _content.UpdateAffiliate(null, new Guid(id), Request.GetIPAddress(), true);
            }
            catch { }
            if (string.IsNullOrWhiteSpace(name))
                return new RedirectResult(VirtualPathUtility.ToAbsolute("~/"));
            else
            {
                try
                {
                    return new RedirectResult(VirtualPathUtility.ToAbsolute(string.Format("~/{0}", Server.UrlDecode(name)).ToLowerInvariant().Replace("/refer/", "/")));
                }
                catch
                {
                    return HttpNotFound();
                }
            }
        }

        [Themed(false)]
        [ValidateInput(false)]
        public ActionResult Referral(string id, string name)
        {
            try
            {
                return new JsonHelper.JsonNetResult(_content.UpdateAffiliate(null, null, Request.GetIPAddress()).AffiliateID, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return new JsonHelper.JsonNetResult(string.Empty, JsonRequestBehavior.AllowGet);
            }
        }

        [Themed(false)]
        [ValidateInput(false)]
        public ActionResult Checkin(string id)
        {
            string nid = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (id.Last() == '=' && (id.Length % 4 == 0) && Regex.IsMatch(id, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                {
                    try
                    {
                        nid = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(id));
                    }
                    catch
                    {
                        nid = id;
                    }
                }
                else
                    nid = id;
            }
            _content.UpdateAffiliate(null, null, Request.GetIPAddress(), false, true, nid);
            return new EmptyResult();
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
        [Themed(false)]
        //[Authorize]
        public ActionResult PickFile(PagerParameters pagerParameters, string q = "")
        {
            return View(new ViewModels.PickFileViewModel { 
                Query=q,  
                SearchResults= _share.GetFiles(q, pagerParameters.Page*pagerParameters.PageSize, pagerParameters.PageSize)});
        }


        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        [Themed(false)]
        //[Authorize]
        public ActionResult MyFiles(string q = "")
        {
            int page;
            int pageSize;
            string query = Request.Params["keywords"];
            if (string.IsNullOrWhiteSpace(query) || query == "undefined")
                query = null;
            bool pFound = int.TryParse(Request.Params["page"], out page);
            bool psFound = int.TryParse(Request.Params["pageSize"], out pageSize);
            return new JsonHelper.JsonNetResult(new { myFiles = _share.GetFiles(query, (pFound && psFound) ? (page * pageSize) + 1 : default(int?), psFound ? pageSize : default(int?) ) }, JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        [Themed(false)]
        //[Authorize]
        public ActionResult PickLocation(PagerParameters pagerParameters, string q = "")
        {
            return View(new ViewModels.PickLocationViewModel
            {
                Query = q,
                SearchResults =  _share.GetLocations(q, pagerParameters.Page * pagerParameters.PageSize, pagerParameters.PageSize)
            });
        }


        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        [Themed(false)]
        //[Authorize]
        [HttpGet]
        [ActionName("MyLocations")]
        public ActionResult GetMyLocations(string q = "")
        {
            int page;
            int pageSize;
            string query = Request.Params["keywords"];
            if (string.IsNullOrWhiteSpace(query) || query == "undefined")
                query = null;
            bool pFound = int.TryParse(Request.Params["page"], out page);
            bool psFound = int.TryParse(Request.Params["pageSize"], out pageSize);
            return new JsonHelper.JsonNetResult(new { myLocations = 
                _share.GetLocations(query, (pFound && psFound) ? (page * pageSize) + 1 : default(int?), psFound ? pageSize : default(int?)) }
                , JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Themed(false)]
        //[Authorize]
        [HttpPost]
        [ActionName("MyLocations")]
        public ActionResult UpdateMyLocation(string q = "")
        {            
            string id = Request.Params["myLocation[LocationID]"];
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.ExpectationFailed);
            string name = Request.Params["myLocation[Title]"];
            string geo = Request.Params["myLocation[Geography]"];
            string culture = (Request.UserLanguages == null || Request.UserLanguages.Length == 0) ? "en-US" : Request.UserLanguages[0];
            var success = _share.SubmitLocation(guid, name, geo, culture);
            if (!success)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.ExpectationFailed);
            return new JsonHelper.JsonNetResult(new
            {
                myLocations = true
            }
                , JsonRequestBehavior.AllowGet);
            

        }


        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        [Themed(true)]
        //[Authorize]
        [HttpGet]
        [ActionName("Location")]
        public ActionResult Location(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return new HttpNotFoundResult();
            var m = _share.GetLocation(guid);
            if (m == null)
                return new HttpNotFoundResult();
            return View(m);
        }



        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Themed(true)]
        //[Authorize]
        [HttpPost]
        [ActionName("Location")]
        public ActionResult SubmitLocation(EXPEDIT.Share.ViewModels.PickLocationViewModel m)
        {
            if (!_share.SubmitLocation(m))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.ExpectationFailed);
            else
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }


        [ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        [Themed(false)]
        //[Authorize]
        public ActionResult LocationRaw(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return new HttpNotFoundResult();
            var m = _share.GetLocation(guid);
            if (m == null)
                return new HttpNotFoundResult();
            return View(m);
        }


        //[Authorize]
        [Themed(Enabled = false)]
        public virtual ActionResult UploadFile()
        {
            //var sessionID = Request.Params["SessionID"];
            //use contactID
            var m = new EXPEDIT.Share.ViewModels.PickFileViewModel { };
            var rFiles = new Dictionary<Guid, HttpPostedFileBase>();
            var rFileLengths = new Dictionary<Guid, int>();
            m.QueryFileLengths = new Dictionary<Guid, int>();
            if (m.QueryFiles == null)
                m.QueryFiles = new Dictionary<Guid, HttpPostedFileBase>();
            for (int i = 0; i < Request.Files.Count; i++)
                m.QueryFiles.Add(Guid.NewGuid(), Request.Files[i]);
            _share.SubmitFiles(m.QueryFiles, m.QueryFileLengths);
            rFiles = m.QueryFiles;
            rFileLengths = m.QueryFileLengths;
            var list = new List<dynamic>();
            foreach (var f in rFiles)
                list.Add(Build<ExpandoObject>.NewObject(name: f.Value.FileName, type: "application/octet", size: rFileLengths[f.Key], url: VirtualPathUtility.ToAbsolute(string.Format("~/share/file/{0}", f.Key))));
            return new JsonHelper.JsonNetResult(new { files = list.ToArray() }, JsonRequestBehavior.AllowGet);
        }


        [Themed(false)]
        [HttpGet]
        [ActionName("MyInfo")]
        public ActionResult GetMyInfo(string id)
        {
            return new JsonHelper.JsonNetResult(_share.GetMyInfo(), JsonRequestBehavior.AllowGet);
        }

        [Themed(false)]
        [HttpGet]
        [ActionName("LoggedIn")]
        public ActionResult LoggedIn()
        {
            return new JsonHelper.JsonNetResult(User.Identity.IsAuthenticated, JsonRequestBehavior.AllowGet);
        }

    }
}
