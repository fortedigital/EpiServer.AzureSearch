using System;
using System.Linq;
using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexGarbageCollector<T> : IIndexGarbageCollector where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;

        public IndexGarbageCollector(IAzureSearchService azureSearchService)
        {
            _azureSearchService = azureSearchService;
        }

        public async Task RemoveOutdatedContent(DateTimeOffset olderThan)
        {
            const int topResults = 1000;
            var query = new AzureSearchQueryBuilder()
                .Top(topResults)
                .Filter(AzureSearchQueryFilter.LessThan(nameof(ContentDocument.IndexedAt), new DateTimeOffsetPropertyValue(olderThan)))
                .Build();

            var deletedDocumentsCount = await DeleteDocuments(query);

            if (deletedDocumentsCount == topResults)
            {
                do
                {
                    deletedDocumentsCount = await DeleteDocuments(query);
                } while (deletedDocumentsCount > 0);
            }
        }

        private async Task<int> DeleteDocuments(AzureSearchQuery query)
        {
            var defaultIndexName = _azureSearchService.GetDefaultIndexName<T>();
            var documentsOlderThan = await _azureSearchService.SearchAsync<SearchDocument>(query, defaultIndexName);

            await _azureSearchService.DeleteAsync(documentsOlderThan.Results.Select(r => r.Document).ToArray(), defaultIndexName);

            return documentsOlderThan.Results.Count;
        }
    }
}
