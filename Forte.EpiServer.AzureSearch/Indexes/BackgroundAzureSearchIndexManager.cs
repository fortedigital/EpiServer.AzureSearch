using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Microsoft.Extensions.Logging;

namespace Forte.EpiServer.AzureSearch.Indexes
{
    public class BackgroundAzureSearchIndexManager : IAzureSearchIndexManager
    {
        private readonly AzureSearchIndexManager _azureSearchIndexManager;
        private readonly ILogger<BackgroundAzureSearchIndexManager> _logger;

        public BackgroundAzureSearchIndexManager(AzureSearchIndexManager azureSearchIndexManager, ILogger<BackgroundAzureSearchIndexManager> logger)
        {
            _azureSearchIndexManager = azureSearchIndexManager;
            _logger = logger;
        }

        public Task CreateOrUpdateIndexAsync<TDocument>() where TDocument : ContentDocument =>
            Task.Run(() => _azureSearchIndexManager.CreateOrUpdateIndexAsync<TDocument>()
                .ContinueWith(t => { _logger.LogError(t.Exception, "Error when creating search index"); }, TaskContinuationOptions.OnlyOnFaulted));
    }
}
