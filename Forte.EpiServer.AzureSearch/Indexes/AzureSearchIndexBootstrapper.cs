using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Configuration;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Indexes
{
    public class AzureSearchIndexBootstrapper : IAzureSearchIndexBootstrapper
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IIndexSpecificationProvider _indexSpecificationProvider;

        public AzureSearchIndexBootstrapper(IAzureSearchService azureSearchService, IIndexSpecificationProvider indexSpecificationProvider)
        {
            _azureSearchService = azureSearchService;
            _indexSpecificationProvider = indexSpecificationProvider;
        }

        public Task CreateOrUpdateIndexAsync<TDocument>() where TDocument : ContentDocument =>
            _azureSearchService.CreateOrUpdateIndexAsync<TDocument>(_indexSpecificationProvider.GetIndexSpecification());
    }
}
