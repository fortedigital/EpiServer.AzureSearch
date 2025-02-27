using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin.Filters
{
    /// <summary>
    /// Filter for excluding shortcut pages (i.e. those pointing to either other internal or external pages) from indexing.
    /// </summary>
    public class ShortcutFilter : IContentIndexFilter
    {
        public bool ShouldIndexContent(IContent content)
        {
            return content is not PageData pageData || pageData.LinkType is PageShortcutType.Normal or PageShortcutType.FetchData;
        }
    }
}
