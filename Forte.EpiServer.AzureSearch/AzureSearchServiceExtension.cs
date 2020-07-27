using System.Collections.Generic;
using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;
using Microsoft.Azure.Search.Models;

namespace Forte.EpiServer.AzureSearch
{
    public static class AzureSearchServiceExtension
    {
        public static async Task<IReadOnlyCollection<SearchResult<T>>> SearchBatchAsync<T>(this IAzureSearchService searchService, AzureSearchQuery query, string indexName = null, int batchSize = 1000) where T : SearchDocument
        {
            var firstPart = await searchService.SearchAsync<T>(query, indexName).ConfigureAwait(false);
            var results = new List<SearchResult<T>>();
            results.AddRange(firstPart.Results);
            
            if (firstPart.Count <= batchSize)
            {
                return results;
            }

            for (var i = 1; i * batchSize < firstPart.Count; i++)
            {
                var queryClone = (AzureSearchQuery) query.Clone();
                queryClone.Skip = i * batchSize + query.Skip;
                queryClone.Top = batchSize;
                
                var nextPart = await searchService.SearchAsync<T>(queryClone, indexName)
                    .ConfigureAwait(false);
                results.AddRange(nextPart.Results);
            }

            return results;
        }

        public static IReadOnlyCollection<SearchResult<T>> SearchBatch<T>(this IAzureSearchService searchService, AzureSearchQuery query, string indexName = null, int batchSize = 1000) where T : SearchDocument
        {
            return searchService.SearchBatchAsync<T>(query, indexName, batchSize).GetAwaiter().GetResult();
        }
    }
}
