using System.Net;
using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Configuration;
using Forte.EpiServer.AzureSearch.Model;
using Microsoft.Rest.Azure;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexDefinitionHandler<T> : IIndexDefinitionHandler
        where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IIndexSpecificationProvider _indexSpecificationProvider;

        public IndexDefinitionHandler(IAzureSearchService azureSearchService, IIndexSpecificationProvider indexSpecificationProvider)
        {
            _azureSearchService = azureSearchService;
            _indexSpecificationProvider = indexSpecificationProvider;
        }

        public async Task<(UpdateOrRecreateResult result, string recreationReason)> UpdateOrRecreateIndex()
        {
            try
            {
                await _azureSearchService.CreateOrUpdateIndexAsync<T>(_indexSpecificationProvider.GetIndexSpecification());

                return (UpdateOrRecreateResult.Ok, string.Empty);
            }
            catch (CloudException e) when (e.Response.StatusCode == HttpStatusCode.BadRequest)
            {
                await _azureSearchService.DropIndexAsync<T>();
                await _azureSearchService.CreateOrUpdateIndexAsync<T>(_indexSpecificationProvider.GetIndexSpecification());

                return (UpdateOrRecreateResult.Recreated, e.Message);
            }
        }
    }
}
