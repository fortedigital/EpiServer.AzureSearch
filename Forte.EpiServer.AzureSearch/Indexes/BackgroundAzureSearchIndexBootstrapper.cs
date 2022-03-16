using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Microsoft.Extensions.Logging;

namespace Forte.EpiServer.AzureSearch.Indexes
{
    public class BackgroundAzureSearchIndexBootstrapper : IAzureSearchIndexBootstrapper
    {
        private readonly AzureSearchIndexBootstrapper _azureSearchIndexBootstrapper;
        private readonly ILogger<BackgroundAzureSearchIndexBootstrapper> _logger;

        public BackgroundAzureSearchIndexBootstrapper(AzureSearchIndexBootstrapper azureSearchIndexBootstrapper, ILogger<BackgroundAzureSearchIndexBootstrapper> logger)
        {
            _azureSearchIndexBootstrapper = azureSearchIndexBootstrapper;
            _logger = logger;
        }

        public Task CreateOrUpdateIndexAsync<TDocument>() where TDocument : ContentDocument =>
            Task.Run(() => _azureSearchIndexBootstrapper.CreateOrUpdateIndexAsync<TDocument>()
                .ContinueWith(t => _logger.LogError(t.Exception, "Error when creating search index"), TaskContinuationOptions.OnlyOnFaulted));
    }
}
