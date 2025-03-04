using EPiServer.Core;
using EPiServer.Filters;

namespace Forte.EpiServer.AzureSearch.Plugin.Filters
{
    /// <summary>
    /// Filter for excluding unpublished content from indexing.
    /// </summary>
    public class PublishedFilter : IContentIndexFilter
    {
        public bool ShouldIndexContent(IContent content)
        {
            var filterPublished = new FilterPublished();

            return !filterPublished.ShouldFilter(content);
        }
    }
}
