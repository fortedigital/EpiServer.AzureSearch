using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Shell.Search;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;
using Microsoft.Azure.Search.Models;

namespace Forte.EpiServer.AzureSearch.SearchProvider
{
    public abstract class ContentSearchProviderBase<T> : ISearchProvider where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentLanguageAccessor _contentLanguageAccessor;

        protected ContentSearchProviderBase(IAzureSearchService azureSearchService, IContentLanguageAccessor contentLanguageAccessor)
        {
            _azureSearchService = azureSearchService;
            _contentLanguageAccessor = contentLanguageAccessor;
        }

        public IEnumerable<SearchResult> Search(EPiServer.Shell.Search.Query query)
        {
            var azureSearchQuery = CreateSearchQuery(query, _contentLanguageAccessor.Language);
            var searchResults = _azureSearchService.Search<T>(azureSearchQuery);

            return searchResults.Results.Select(MapAzureSearchResult);
        }

        private static SearchResult MapAzureSearchResult(SearchResult<T> result)
        {
            var previewText = string.Empty;

            if (result.Highlights != null && result.Highlights.Count > 0)
            {
                previewText = result.Highlights.First().Value.First();
            }

            var editModeUrl = PageEditing.GetEditUrlForLanguage(new ContentReference(result.Document.ContentComplexReference), result.Document.ContentLanguage); 
            
            return new SearchResult(editModeUrl, result.Document.ContentName, previewText)
            {
                Metadata =
                {
                    new KeyValuePair<string, string>("languageBranch", result.Document.ContentLanguage),
                    new KeyValuePair<string, string>("id", result.Document.ContentComplexReference)
                }
            };
        }

        private static AzureSearchQuery CreateSearchQuery(EPiServer.Shell.Search.Query query, CultureInfo currentCulture)
        {
            var queryBuilder = new AzureSearchQueryBuilder()
                .Top(query.MaxResults)
                .SearchTerm(query.SearchQuery);

            if (query.FilterOnCulture)
            {
                queryBuilder.Filter(AzureSearchQueryFilter.Equals(nameof(ContentDocument.ContentLanguage),
                    currentCulture.Name));
            }
            
            return queryBuilder.Build();
        }

        public abstract string Area { get; }
        public abstract string Category { get; }
    }
}
