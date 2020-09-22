using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class PageSearchEventHandler<T> where T : ContentDocument
    {
        private readonly IContentLoader _contentLoader;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        
        public PageSearchEventHandler(IContentLoader contentLoader, IContentDocumentBuilder<T> contentDocumentBuilder)
        {
            _contentLoader = contentLoader;
            _contentDocumentBuilder = contentDocumentBuilder;
        }

        public IEnumerable<T> GetPageContentDocuments(ContentReference contentLink, bool includeDeleted = false)
        {
            var contentInAllLanguages = _contentLoader.GetAllLanguageVersions(contentLink);
            var documents = contentInAllLanguages
                .Where(c =>  (includeDeleted && c.IsDeleted) || c.ShouldIndexPage())
                .Select(_contentDocumentBuilder.Build)
                .ToList();
            return documents;
        }
        
        public T GetSpecificPageVersionContentDocument(ContentReference contentLink)
        {
            var content = _contentLoader.Get<IContent>(contentLink);
            var document = _contentDocumentBuilder.Build(content);
            return document;
        }

        public IEnumerable<T> GetDocumentsToReindex(IContent root, bool includeDeleted = false)
        {
            var listResult = new List<T>();

            listResult.AddRange(GetPageContentDocuments(root.ContentLink, includeDeleted));

            var descendants = _contentLoader.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                listResult.AddRange(GetPageContentDocuments(descendant, includeDeleted));
            }

            return listResult;
        }

    }
}
