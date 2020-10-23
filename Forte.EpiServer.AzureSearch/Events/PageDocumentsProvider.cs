using System;
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
    public class PageDocumentsProvider<T> where T : ContentDocument
    {
        public const string OldUrlKey = "OldUrl";
        private readonly IContentLoader _contentLoader;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        private readonly IUrlResolver _urlResolver;

        public PageDocumentsProvider(IContentLoader contentLoader, IContentDocumentBuilder<T> contentDocumentBuilder,
            IUrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _contentDocumentBuilder = contentDocumentBuilder;
            _urlResolver = urlResolver;
        }

        public IReadOnlyCollection<T> GetDocuments(IContent page, ContentEventArgs contentEventArgs)
        {
            var oldUrl = contentEventArgs.Items[OldUrlKey]?.ToString();
            var newUrl = _urlResolver.GetUrl(contentEventArgs.ContentLink);
            
            var isUrlChanged = oldUrl != newUrl;
            var isMasterLanguage = page.IsMasterLanguageBranch();

            var documents = isUrlChanged
                ? GetPageTreeDocuments(page, isMasterLanguage)
                : GetPageDocuments(page, isMasterLanguage);

            return documents.ToList();
        }

        public IReadOnlyCollection<T> GetPageTreeDocuments(IContent root, bool isMasterLanguage)
        {
            return isMasterLanguage
                ? GetPageTreeAllLanguagesDocuments(root)
                : GetPageTreeSpecificLanguageDocuments(root);
        }

        private IReadOnlyCollection<T> GetPageDocuments(IContent page, bool isMasterLanguage)
        {
            return isMasterLanguage 
                ? GetPageAllLanguagesContentDocuments(page.ContentLink)
                : GetPageVersionContentDocument(page.ContentLink);
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
        
        private IReadOnlyCollection<T> GetPageTreeSpecificLanguageDocuments(IContent root)
        {
            var language = ((ILocalizable) root).Language;
            var results = new List<T>();
            
            var document = GetPageLanguageBranchContentDocument(root.ContentLink, language);
            if (document != null)
            {
                results.Add(document);
            }

            var descendants = _contentLoader.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                document = GetPageLanguageBranchContentDocument(descendant, language);
                if (document != null)
                {
                    results.Add(document);
                }
            }

            return results;
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

            if (content.ShouldIndexPage() == false)
            {
                return Array.Empty<T>();
            }
            var document = _contentDocumentBuilder.Build(content);
            return new[] { document };
        }

        private T GetPageLanguageBranchContentDocument(ContentReference contentLink, CultureInfo language)
        {
            var content = _contentLoader.Get<IContent>(new ContentReference(contentLink.ID), language);
            
            if (content.ShouldIndexPage() == false)
            {
                return null;
            }
            var document = _contentDocumentBuilder.Build(content);
            return document;
        }
    }
}
