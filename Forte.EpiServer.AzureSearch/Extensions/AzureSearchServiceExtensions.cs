using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;
using Microsoft.Azure.Search.Models;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class AzureSearchServiceExtensions
    {
        public static async Task<DocumentIndexResult> IndexAsync<T>(this IAzureSearchService azureSearchService, T document, string indexName = null) where T : SearchDocument
        {
            return await azureSearchService.IndexAsync(new [] {document}, indexName);
        }
        
        public static DocumentIndexResult Index<T>(this IAzureSearchService azureSearchService, T document, string indexName = null) where T : SearchDocument
        {
            return azureSearchService.Index(new[] {document}, indexName);
        }
    }
}
