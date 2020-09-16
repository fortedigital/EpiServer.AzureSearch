using Forte.EpiServer.AzureSearch.ContentExtractor.Block;
using Forte.EpiServer.AzureSearch.ContentExtractor.Page;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public interface ISearchableContentExtractor : IPageContentExtractor, IBlockContentExtractor
    {
    }
}
