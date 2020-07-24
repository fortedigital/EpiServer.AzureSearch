using System.Collections.Generic;
using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;
using Microsoft.Azure.Search.Models;

namespace Forte.EpiServer.AzureSearch
{
    public class BulkSearchHelper
    {
        private const int MaxSearchResultCount = 1000;
        private readonly IAzureSearchService _searchService;

        public BulkSearchHelper(IAzureSearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task<IReadOnlyCollection<DocumentSearchResult<T>>> SearchAsync<T>(AzureSearchQuery query, string indexName = null) where T : SearchDocument
        {
            var firstPart = await _searchService.SearchAsync<T>(query, indexName)
                .ConfigureAwait(false);
            var results = new List<DocumentSearchResult<T>>{ firstPart };
            if (firstPart.Count <= MaxSearchResultCount)
            {
                return results;
            }

            var originalSkip = query.Skip;
            query.Top = MaxSearchResultCount;
            
            for (var i = 1; i * MaxSearchResultCount < firstPart.Count; i++)
            {
                query.Skip = i * MaxSearchResultCount + originalSkip;
                var nextPart = await _searchService.SearchAsync<T>(query, indexName);
                results.Add(nextPart);
            }

            return results;
        }

        public IReadOnlyCollection<DocumentSearchResult<T>> Search<T>(AzureSearchQuery query, string indexName = null) where T : SearchDocument
        {
            return SearchAsync<T>(query, indexName).GetAwaiter().GetResult();
        }
    }
}
