using Microsoft.Azure.Search.Models;

namespace Forte.EpiServer.AzureSearch
{
    public interface IIndexSpecification
    {
        void Setup(Index index);
    }
}