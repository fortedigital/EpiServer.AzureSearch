using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Search.Models;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Query;

namespace Forte.EpiServer.AzureSearch
{
    //TODO: separate this interface into smaller ones
    public interface IAzureSearchService
    {
        string GetDefaultIndexName<T>()
            where T : SearchDocument;

        Task<DocumentIndexResult> IndexAsync<T>(IEnumerable<T> documents, string indexName = null)
            where T : SearchDocument;

        DocumentIndexResult Index<T>(IEnumerable<T> documents, string indexName = null)
            where T : SearchDocument;

        Task<bool> IndexExistsAsync<T>()
            where T : SearchDocument;

        bool IndexExists<T>()
            where T : SearchDocument;

        Task CreateOrUpdateIndexAsync<T>(IIndexSpecification indexSpecification = null)
            where T : SearchDocument;

        void CreateOrUpdateIndex<T>(IIndexSpecification indexSpecification = null)
            where T : SearchDocument;

        Task<DocumentIndexResult> DeleteAsync<T>(IEnumerable<T> documents, string indexName = null)
            where T : SearchDocument;

        void Delete<T>(IEnumerable<T> documents, string indexName = null)
            where T : SearchDocument;

        Task DropIndexAsync<T>()
            where T : SearchDocument;

        void DropIndex<T>()
            where T : SearchDocument;

        Task<DocumentSearchResult<T>> SearchAsync<T>(AzureSearchQuery query, string indexName = null)
            where T : SearchDocument;

        DocumentSearchResult<T> Search<T>(AzureSearchQuery query, string indexName = null)
            where T : SearchDocument;
    }
}
