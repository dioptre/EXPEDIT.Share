using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;
using Orchard;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Transactions;
using Orchard.Messaging.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Data;
#if NKD
using NKD.Module.BusinessObjects;
#else
using EXPEDIT.Utils.DAL.Models;
#endif
using NKD.Services;
using Orchard.Media.Services;
using EXPEDIT.Share.Helpers;
using System.Web.Mvc;
using NKD.Helpers;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

using System;
using System.Globalization;
using System.Linq;
using Orchard.Collections;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Indexing;
using Orchard.Search.Services;

namespace EXPEDIT.Share.Services {
    
    [UsedImplicitly]
    public class SecureSearchService : ISearchService
    {
        private readonly IIndexManager _indexManager;
        private readonly ICultureManager _cultureManager;

        public SecureSearchService(IOrchardServices services, IIndexManager indexManager, ICultureManager cultureManager)
        {
            Services = services;
            _indexManager = indexManager;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        ISearchBuilder Search()
        {
            return _indexManager.HasIndexProvider()
                ? _indexManager.GetSearchIndexProvider().CreateSearchBuilder("Search")
                : new NullSearchBuilder();
        }

        IPageOfItems<T> ISearchService.Query<T>(string query, int page, int? pageSize, bool filterCulture, string[] searchFields, Func<ISearchHit, T> shapeResult)
        {

            if (string.IsNullOrWhiteSpace(query))
                return new PageOfItems<T>(Enumerable.Empty<T>());

            var searchBuilder = Search().Parse(searchFields, query);

            if (filterCulture)
            {
                var culture = _cultureManager.GetSiteCulture();

                // use LCID as the text representation gets analyzed by the query parser
                searchBuilder
                    .WithField("culture", CultureInfo.GetCultureInfo(culture).LCID)
                    .AsFilter();
            }

            var totalCount = searchBuilder.Count();
            if (pageSize != null)
                searchBuilder = searchBuilder
                    .Slice((page > 0 ? page - 1 : 0) * (int)pageSize, (int)pageSize);

            var searchResults = searchBuilder.Search();

            var pageOfItems = new PageOfItems<T>(searchResults.Select(shapeResult))
            {
                PageNumber = page,
                PageSize = pageSize != null ? (int)pageSize : totalCount,
                TotalItemCount = totalCount
            };

            return pageOfItems;
        }
    }
}
