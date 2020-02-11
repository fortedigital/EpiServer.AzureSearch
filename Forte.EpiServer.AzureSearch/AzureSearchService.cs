using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Logging;
using Forte.EpiServer.AzureSearch.Configuration;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Polly;

namespace Forte.EpiServer.AzureSearch
{
    public class AzureSearchService : IAzureSearchService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IIndexNamingConvention _indexNamingConvention;
        private readonly SearchServiceClient _client;

        public const int BatchMaximumSize = 1000;

        public AzureSearchService(AzureSearchServiceConfiguration configuration, ILogger logger, IIndexNamingConvention indexNamingConvention)
        {
            _logger = logger;
            _indexNamingConvention = indexNamingConvention;
            _client = new SearchServiceClient(configuration.ServiceName, new SearchCredentials(configuration.ApiKey));
        }

        public async Task<DocumentSearchResult<T>> SearchAsync<T>(AzureSearchQuery query, string indexName = null) where T : SearchDocument
        {
            var searchParameters = BuildSearchParameters(query);

            return await _client
                .Indexes
                .GetClient(indexName ?? GetDefaultIndexName<T>())
                .Documents
                .SearchAsync<T>(query.SearchTerm + "*", searchParameters)
                .ConfigureAwait(false); 
        }

        public DocumentSearchResult<T> Search<T>(AzureSearchQuery query, string indexName = null) where T : SearchDocument
        {
            return SearchAsync<T>(query, indexName).GetAwaiter().GetResult();
        }

        public async Task<DocumentIndexResult> DeleteAsync<T>(IEnumerable<T> documents, string indexName = null) where T : SearchDocument
        {
            return await IndexBatchWithRetryAsync(IndexAction.Delete, documents,  indexName);
        }

        public void Delete<T>(IEnumerable<T> documents, string indexName = null) where T : SearchDocument
        {
            DeleteAsync(documents, indexName).GetAwaiter().GetResult();
        }

        private static SearchParameters BuildSearchParameters(AzureSearchQuery query)
        {
            return new SearchParameters
            {
                Skip = query.Skip,
                Top = query.Top,
                HighlightFields = query.HighlightFields,
                HighlightPreTag = query.HighlightPreTag,
                HighlightPostTag = query.HighlightPostTag,
                Facets = query.Facets,
                Filter = query.Filter,
                IncludeTotalResultCount = true,
                ScoringProfile = query.ScoringProfile,
            };
        }
        
        public async Task<DocumentIndexResult> IndexAsync<T>(IEnumerable<T> documents, string indexName = null) where T : SearchDocument
        {
            return await IndexBatchWithRetryAsync(IndexAction.Upload, documents, indexName);
        }

        private async Task<DocumentIndexResult> IndexBatchWithRetryAsync<T>(Func<T, IndexAction<T>> indexActionFunc, IEnumerable<T> documents, string indexName) where T : SearchDocument
        {
            const int retryCount = 2;
            var documentsArray = documents.ToArray();
            var documentsChunks = documentsArray.Chunk(BatchMaximumSize);
            var indexBatches = documentsChunks.Select(chunk => IndexBatch.New(chunk.Select(indexActionFunc)));
            var indexingResults = new List<IndexingResult>();

            foreach (var indexBatch in indexBatches)
            {
                var policy = Policy.Handle<IndexBatchException>().WaitAndRetryAsync(retryCount,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    async (exception, span) =>
                    {
                        var indexBatchException = (IndexBatchException) exception;
                        var itemsToRetry = indexBatchException.FindFailedActionsToRetry(indexBatch, d => d.Id);

                        var now = DateTimeOffset.UtcNow;
                        Array.ForEach(documentsArray, d => d.IndexedAt = now);
                        
                        await ExecuteIndexBatchAsync(itemsToRetry, indexName);
                    });

                try
                {
                    var indexBatchResult = await policy.ExecuteAsync(async () =>
                    {
                        var now = DateTimeOffset.UtcNow;
        
                        Array.ForEach(documentsArray, d => d.IndexedAt = now);
            
                        return await ExecuteIndexBatchAsync(indexBatch, indexName);
                    });
                
                    indexingResults.AddRange(indexBatchResult.Results);
                }
                catch (Exception e)
                {
                    _logger.Log(Level.Error, "Exception when indexing content", e);
                }
            }
            
            return new DocumentIndexResult(indexingResults);
        }
        
        public DocumentIndexResult Index<T>(IEnumerable<T> documents, string indexName = null) where T : SearchDocument
        {
            return IndexAsync(documents, indexName).GetAwaiter().GetResult();
        }

        public async Task<bool> IndexExistsAsync<T>() where T : SearchDocument
        {
            return await _client
                .Indexes
                .ExistsAsync(GetDefaultIndexName<T>())
                .ConfigureAwait(false);
        }

        public bool IndexExists<T>() where T : SearchDocument
        {
            return IndexExistsAsync<T>().GetAwaiter().GetResult();
        }

        public void CreateOrUpdateIndex<T>(IIndexSpecification indexSpecification = null) where T : SearchDocument
        {
            CreateOrUpdateIndexAsync<T>(indexSpecification).GetAwaiter().GetResult();
        }

        public void DropIndex<T>() where T : SearchDocument
        {
            DropIndexAsync<T>().GetAwaiter().GetResult();
        }
        
        public async Task DropIndexAsync<T>() where T : SearchDocument
        {
            var indexName = GetDefaultIndexName<T>();

            await _client
                .Indexes
                .DeleteAsync(indexName)
                .ConfigureAwait(false);
        }

        public async Task CreateOrUpdateIndexAsync<T>(IIndexSpecification indexSpecification = null) where T : SearchDocument
        {
            var indexDefinition = new Index
            {
                Name = GetDefaultIndexName<T>(),
                Fields = FieldBuilder.BuildForType<T>(),
            };
            
            indexSpecification?.Setup(indexDefinition);

            await _client
                .Indexes
                .CreateOrUpdateAsync(indexDefinition)
                .ConfigureAwait(false);
        }

        private async Task<DocumentIndexResult> ExecuteIndexBatchAsync<T>(IndexBatch<T> indexBatch, string indexName) where T : SearchDocument
        {
            return await _client
                .Indexes
                .GetClient(indexName ?? GetDefaultIndexName<T>())
                .Documents
                .IndexAsync(indexBatch)
                .ConfigureAwait(false);                
        }

        public string GetDefaultIndexName<T>() where T : SearchDocument
        {
            return _indexNamingConvention.GetIndexName(typeof(T).Name);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
