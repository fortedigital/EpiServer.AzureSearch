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
        private readonly IContentRepository _contentRepository;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        
        public PageSearchEventHandler(IContentRepository contentRepository, IContentDocumentBuilder<T> contentDocumentBuilder)
        {
            _contentRepository = contentRepository;
            _contentDocumentBuilder = contentDocumentBuilder;
        }

        public IEnumerable<T> GetPageContentDocuments(ContentReference contentLink, bool includeDeleted = false)
        {
            var contentInAllLanguages = _contentRepository.GetAllLanguageVersions(contentLink);
            var documents = contentInAllLanguages
                .Where(c => (includeDeleted && c.IsDeleted) || c.ShouldPageIndex())
                .Select(_contentDocumentBuilder.Build)
                .ToList();
            return documents;
        }
        
        public IEnumerable<T> GetDocumentsToReindex(IContent root, bool includeDeleted = false)
        {
            var listResult = new List<T>();

            listResult.AddRange(GetPageContentDocuments(root.ContentLink, includeDeleted));

            var descendants = _contentRepository.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                listResult.AddRange(GetPageContentDocuments(descendant, includeDeleted));
            }

            return listResult;
        }

    }
}
