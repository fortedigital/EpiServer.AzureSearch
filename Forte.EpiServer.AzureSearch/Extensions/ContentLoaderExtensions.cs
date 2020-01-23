using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class ContentLoaderExtensions
    {
        public static IEnumerable<IContent> GetAllLanguageVersions(this IContentLoader contentLoader, ContentReference contentReference)
        {
            var rootPage = contentLoader.Get<PageData>(contentReference);
            
            foreach (var cultureInfo in rootPage.ExistingLanguages)
            {
                var loaderOptions = new LoaderOptions {LanguageLoaderOption.Specific(cultureInfo)};
                var pageInSpecificLanguage = contentLoader.Get<PageData>(contentReference, loaderOptions);
                
                yield return pageInSpecificLanguage;
            }
        }
    }
}
