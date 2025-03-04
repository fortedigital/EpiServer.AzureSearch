using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin.Filters
{
    public interface IContentIndexFilter
    {
        bool ShouldIndexContent(IContent content);
    }
}
