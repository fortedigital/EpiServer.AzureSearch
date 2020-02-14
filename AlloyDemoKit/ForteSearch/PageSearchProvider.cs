using EPiServer.Core;
using EPiServer.Shell.Search;
using Forte.EpiServer.AzureSearch;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.SearchProvider;

namespace AlloyDemoKit.ForteSearch
{
    [SearchProvider]
    public class PageSearchProvider : ContentSearchProviderBase<ContentDocument>
    {
        public PageSearchProvider(IAzureSearchService azureSearchService, IContentLanguageAccessor contentLanguageAccessor) : base(azureSearchService, contentLanguageAccessor)
        {
        }

        public override string Area => "CMS/pages";

        public override string Category => "Find pages";
    }
}
