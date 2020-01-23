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

        public async Task<DocumentSearchResult<T>> SearchAsync<T>(AzureSearchQuery query) where T : SearchDocument
        {
            var indexName = GetIndexName<T>();

            var searchParameters = BuildSearchParameters(query);

            return await _client
                .Indexes
                .GetClient(indexName)
                .Documents
                .SearchAsync<T>(query.SearchTerm + "*", searchParameters)
                .ConfigureAwait(false);
        }

        public DocumentSearchResult<T> Search<T>(AzureSearchQuery query) where T : SearchDocument
        {
            return SearchAsync<T>(query).GetAwaiter().GetResult();
        }

        public async Task<DocumentIndexResult> DeleteAsync<T>(params T[] documents) where T : SearchDocument
        {
            return await IndexBatchWithRetryAsync(IndexAction.Delete, documents);
        }

        public void Delete<T>(params T[] documents) where T : SearchDocument
        {
            DeleteAsync(documents).GetAwaiter().GetResult();
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
        
        public async Task<DocumentIndexResult> IndexAsync<T>(params T[] documents) where T : SearchDocument
        {
            return await IndexBatchWithRetryAsync(IndexAction.Upload, documents);
        }

        private async Task<DocumentIndexResult> IndexBatchWithRetryAsync<T>(Func<T, IndexAction<T>> indexActionFunc, params T[] documents) where T : SearchDocument
        {
            const int retryCount = 2;
            var documentsChunks = documents.Chunk(BatchMaximumSize);
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
                        Array.ForEach(documents, d => d.IndexedAt = now);
                        
                        await ExecuteIndexBatchAsync(itemsToRetry);
                    });

                try
                {
                    var indexBatchResult = await policy.ExecuteAsync(async () =>
                    {
                        var now = DateTimeOffset.UtcNow;
        
                        Array.ForEach(documents, d => d.IndexedAt = now);
            
                        return await ExecuteIndexBatchAsync(indexBatch);
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
        
        
        public DocumentIndexResult Index<T>(params T[] documents) where T : SearchDocument
        {
            return IndexAsync(documents).GetAwaiter().GetResult();
        }

        public async Task<bool> IndexExistsAsync<T>() where T : SearchDocument
        {
            return await _client
                .Indexes
                .ExistsAsync(GetIndexName<T>())
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
            var indexName = GetIndexName<T>();

            await _client
                .Indexes
                .DeleteAsync(indexName)
                .ConfigureAwait(false);
        }

        public async Task CreateOrUpdateIndexAsync<T>(IIndexSpecification indexSpecification = null) where T : SearchDocument
        {
            var indexDefinition = new Index
            {
                Name = GetIndexName<T>(),
                Fields = FieldBuilder.BuildForType<T>(),
            };
            
            indexSpecification?.Setup(indexDefinition);

            await _client
                .Indexes
                .CreateOrUpdateAsync(indexDefinition)
                .ConfigureAwait(false);
        }

        private async Task<DocumentIndexResult> ExecuteIndexBatchAsync<T>(IndexBatch<T> indexBatch) where T : SearchDocument
        {
            var indexName = GetIndexName<T>();

            return await _client
                .Indexes
                .GetClient(indexName)
                .Documents
                .IndexAsync(indexBatch)
                .ConfigureAwait(false);                
        }

        protected string GetIndexName<T>() where T : SearchDocument
        {
            return _indexNamingConvention.GetIndexName(typeof(T).Name);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
