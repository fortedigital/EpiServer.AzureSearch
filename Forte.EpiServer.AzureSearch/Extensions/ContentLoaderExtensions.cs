using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class ContentLoaderExtensions
    {
        public static IEnumerable<IContent> GetAllLanguageVersions(this IContentLoader contentLoader, ContentReference contentReference)
        {
            var content = contentLoader.Get<IContent>(contentReference);

            if (content is ILocalizable rootContent)
            {
                foreach (var cultureInfo in rootContent.ExistingLanguages)
                {
                    var loaderOptions = new LoaderOptions { LanguageLoaderOption.Specific(cultureInfo) };
                    var contentInSpecificLanguage = contentLoader.Get<IContent>(contentReference.ToReferenceWithoutVersion(), loaderOptions);

                    yield return contentInSpecificLanguage;
                }
            }
            else
            {
                yield return content;
            }
        }
    }
}
