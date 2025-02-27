using EPiServer.Core;
using EPiServer.Filters;

namespace Forte.EpiServer.AzureSearch.Plugin.Filters
{
    /// <summary>
    /// Filter for excluding pages that does not have a page template defined from indexing.
    /// </summary>
    public class TemplateFilter : IContentIndexFilter
    {
        public bool ShouldIndexContent(IContent content)
        {
            var filterTemplate = new FilterTemplate();

            return !filterTemplate.ShouldFilter(content);
        }
    }
}
