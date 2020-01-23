using System.Threading.Tasks;
using Microsoft.Azure.Search.Models;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;

namespace Forte.EpiServer.AzureSearch
{
    public interface IAzureSearchService
    {
        Task<DocumentIndexResult> IndexAsync<T>(params T[] documents) where T : SearchDocument;
        DocumentIndexResult Index<T>(params T[] documents) where T : SearchDocument;
        Task<bool> IndexExistsAsync<T>() where T : SearchDocument;
        bool IndexExists<T>() where T : SearchDocument;
        Task CreateOrUpdateIndexAsync<T>(IIndexSpecification indexSpecification = null) where T : SearchDocument;
        void CreateOrUpdateIndex<T>(IIndexSpecification indexSpecification = null) where T : SearchDocument;
        Task DropIndexAsync<T>() where T : SearchDocument;
        void DropIndex<T>() where T : SearchDocument;
        Task<DocumentSearchResult<T>> SearchAsync<T>(AzureSearchQuery query) where T : SearchDocument;
        DocumentSearchResult<T> Search<T>(AzureSearchQuery query) where T : SearchDocument;
        Task<DocumentIndexResult> DeleteAsync<T>(params T[] documents) where T : SearchDocument;
        void Delete<T>(params T[] documents) where T : SearchDocument;
    }
}
