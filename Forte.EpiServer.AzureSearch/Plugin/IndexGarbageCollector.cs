using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexGarbageCollector<T> : IIndexGarbageCollector
        where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;

        public IndexGarbageCollector(IAzureSearchService azureSearchService)
        {
            _azureSearchService = azureSearchService;
        }

        public async Task RemoveOutdatedContent(DateTimeOffset olderThan, IList<ContentReference> contentReferencesToPreserve)
        {
            const int maxContentReferencesToPreserve = 1000;

            if (contentReferencesToPreserve.Count > maxContentReferencesToPreserve)
            {
                throw new ArgumentException(
                    $"Number of content references to preserve cannot be greater than {maxContentReferencesToPreserve}",
                    nameof(contentReferencesToPreserve));
            }

            const int topResults = 1000;

            var query = new AzureSearchQueryBuilder()
                .Top(topResults)
                .Filter(
                    new NotFilter(
                        new AzureSearchFilterSearchIn(
                            nameof(ContentDocument.ContentComplexReference),
                            contentReferencesToPreserve.Select(reference => reference.ToString()))))
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
