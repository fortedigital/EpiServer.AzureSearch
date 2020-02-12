using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EPiServer.Core;
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    [ScheduledPlugIn(GUID = "922CADD1-EFA8-43C1-AE15-2FC1D88F1CC9", DisplayName = "[Search] Index content")]
    public class ContentIndexingScheduledJob : ScheduledJobBase
    {
        private readonly IContentIndexer _contentIndexer;
        private readonly IIndexDefinitionHandler _indexDefinitionHandler;
        private readonly IIndexGarbageCollector _indexGarbageCollector;
        private readonly CancellationTokenSource _cancellationToken;
        
        public ContentIndexingScheduledJob(IContentIndexer contentIndexer, IIndexDefinitionHandler indexDefinitionHandler, IIndexGarbageCollector indexGarbageCollector)
        {
            _contentIndexer = contentIndexer;
            _indexDefinitionHandler = indexDefinitionHandler;
            _indexGarbageCollector = indexGarbageCollector;
            _cancellationToken = new CancellationTokenSource();
            IsStoppable = true;
            
        }

        public override string Execute()
        {
            var stopWatch = Stopwatch.StartNew();
            var jobStartTime = DateTimeOffset.UtcNow;
            
            OnStatusChanged("Ensuring valid index definition");
            
            var updateOrRecreateResult = _indexDefinitionHandler.UpdateOrRecreateIndex().GetAwaiter().GetResult();
            
            var message = $"UpdateOrRecreateIndexResult: {updateOrRecreateResult.Type}\n";
            if (updateOrRecreateResult.Type == UpdateOrRecreateResultEnum.Recreated)
            {
                message += $"Recreation reason: {updateOrRecreateResult.RecreationReason}\n";
            }
            
            var indexContentRequest = new IndexContentRequest
            {
                CancellationToken = _cancellationToken,
                OnStatusChanged = OnStatusChanged,
                IgnoreContent = new HashSet<ContentReference>{ContentReference.WasteBasket},
                VisitedContent = new HashSet<ContentReference>(),
                Statistics = new IndexStatistics(),
            };
            
            OnStatusChanged("Indexing content start...");
            _contentIndexer.Index(ContentReference.RootPage, indexContentRequest).GetAwaiter().GetResult();

            if (!_cancellationToken.IsCancellationRequested)
            {
                OnStatusChanged("Clearing outdated items...");
                _indexGarbageCollector.RemoveOutdatedContent(jobStartTime).GetAwaiter().GetResult();                
            }
            
            stopWatch.Stop();
            
            message += indexContentRequest.Statistics.Exceptions.Count >= indexContentRequest.ExceptionThreshold
                          ? $"Exceptions threshold reached. Exceptions: {string.Join("; ", indexContentRequest.Statistics.Exceptions.Select(e => e.ToString()))}, Failed for ids: {string.Join(",", indexContentRequest.Statistics.FailedIds.Select(c => c.ID))}"
                          : $"Content has been indexed. Visited content count: {indexContentRequest.VisitedContent.Count}, Time taken: {stopWatch.Elapsed}";

            return message;
        }

        public override void Stop()
        {
            _cancellationToken.Cancel();
        }
    }
}
