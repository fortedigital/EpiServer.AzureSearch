using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class PageSearchEventHandler<T> where T : ContentDocument
    {
        public const string OldUrlKey = "OldUrl";
        private readonly IContentLoader _contentLoader;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        private readonly IUrlResolver _urlResolver;

        public PageSearchEventHandler(IContentLoader contentLoader, IContentDocumentBuilder<T> contentDocumentBuilder,
            IUrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _contentDocumentBuilder = contentDocumentBuilder;
            _urlResolver = urlResolver;
        }

        public IReadOnlyCollection<T> GetDocuments(IContent page, ContentEventArgs contentEventArgs)
        {
            var oldUrl = contentEventArgs.Items[OldUrlKey].ToString();
            var newUrl = _urlResolver.GetUrl(contentEventArgs.ContentLink);
            
            var isUrlChanged = oldUrl != newUrl;
            var isMasterLanguage = page.IsMasterLanguageBranch();

            var documents = isUrlChanged
                ? isMasterLanguage 
                    ? GetPageTreeAllLanguagesDocuments(page)
                    : GetPageTreeSpecificLanguageDocuments(page)
                : isMasterLanguage 
                    ? GetPageAllLanguagesContentDocuments(page.ContentLink)
                    : GetPageVersionContentDocument(page.ContentLink);

            return documents.ToList();
        }

        private IReadOnlyCollection<T> GetPageAllLanguagesContentDocuments(ContentReference contentLink, bool includeDeleted = false)
        {
            var contentInAllLanguages = _contentLoader.GetAllLanguageVersions(contentLink);
            var documents = contentInAllLanguages
                .Where(c =>  (includeDeleted && c.IsDeleted) || c.ShouldIndexPage())
                .Select(_contentDocumentBuilder.Build)
                .ToList();
            return documents;
        }
        
        public IReadOnlyCollection<T> GetPageVersionContentDocument(ContentReference contentLink)
        {
            var content = _contentLoader.Get<IContent>(contentLink);
            var document = _contentDocumentBuilder.Build(content);
            return new[] { document };
        }

        private T GetPageLanguageBranchContentDocument(ContentReference contentLink, CultureInfo language)
        {
            var content = _contentLoader.Get<IContent>(new ContentReference(contentLink.ID), language);
            var document = _contentDocumentBuilder.Build(content);
            return document;
        }

        public IReadOnlyCollection<T> GetPageTreeAllLanguagesDocuments(IContent root, bool includeDeleted = false)
        {
            var results = new List<T>();

            results.AddRange(GetPageAllLanguagesContentDocuments(root.ContentLink, includeDeleted));

            var descendants = _contentLoader.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                results.AddRange(GetPageAllLanguagesContentDocuments(descendant, includeDeleted));
            }

            return results;
        }
        
        public IReadOnlyCollection<T> GetPageTreeSpecificLanguageDocuments(IContent root)
        {
            var language = ((ILocalizable) root).Language;
            var results = new List<T> { GetPageLanguageBranchContentDocument(root.ContentLink, language) };

            var descendants = _contentLoader.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                results.Add(GetPageLanguageBranchContentDocument(descendant, language));
            }

            return results;
        }

    }
}
