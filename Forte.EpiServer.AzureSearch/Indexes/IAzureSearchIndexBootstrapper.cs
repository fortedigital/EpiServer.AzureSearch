using System.Threading.Tasks;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Indexes
{
    public interface IAzureSearchIndexBootstrapper
    {
        Task CreateOrUpdateIndexAsync<TDocument>() where TDocument : ContentDocument;
    }
}
