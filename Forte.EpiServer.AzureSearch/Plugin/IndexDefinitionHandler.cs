using System.Net;
using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Configuration;
using Forte.EpiServer.AzureSearch.Model;
using Microsoft.Rest.Azure;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexDefinitionHandler<T> : IIndexDefinitionHandler where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IIndexSpecificationProvider _indexSpecificationProvider;

        public IndexDefinitionHandler(IAzureSearchService azureSearchService, IIndexSpecificationProvider indexSpecificationProvider)
        {
            _azureSearchService = azureSearchService;
            _indexSpecificationProvider = indexSpecificationProvider;
        }
        
        public async Task<UpdateOrRecreateResult> UpdateOrRecreateIndex()
        {
            try
            {
                await _azureSearchService.CreateOrUpdateIndexAsync<T>(_indexSpecificationProvider.GetIndexSpecification());

                return new UpdateOrRecreateResult(UpdateOrRecreateResultEnum.Ok, string.Empty);
            }
            catch (CloudException e) when (e.Response.StatusCode == HttpStatusCode.BadRequest)
            {
                await _azureSearchService.DropIndexAsync<T>();
                await _azureSearchService.CreateOrUpdateIndexAsync<T>(_indexSpecificationProvider.GetIndexSpecification());

                return new UpdateOrRecreateResult(UpdateOrRecreateResultEnum.Recreated, e.Message);
            }
        }
    }

}
