using System.Threading.Tasks;
using EPiServer.Logging;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Indexes
{
    public class BackgroundAzureSearchIndexManager : IAzureSearchIndexManager
    {
        private readonly AzureSearchIndexManager _azureSearchIndexManager;
        private readonly ILogger _logger;

        public BackgroundAzureSearchIndexManager(AzureSearchIndexManager azureSearchIndexManager, ILogger logger)
        {
            _azureSearchIndexManager = azureSearchIndexManager;
            _logger = logger;
        }

        public Task CreateOrUpdateIndexAsync<TDocument>() where TDocument : ContentDocument =>
            Task.Run(() => _azureSearchIndexManager.CreateOrUpdateIndexAsync<TDocument>()
                .ContinueWith(t => { _logger.Error("Error when creating search index", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted));
    }
}
