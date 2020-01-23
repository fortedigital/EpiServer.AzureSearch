using System.Threading.Tasks;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public interface IContentIndexer
    {
        Task Index(ContentReference rootContentLink, IndexContentRequest indexContentRequest);
    }
}
