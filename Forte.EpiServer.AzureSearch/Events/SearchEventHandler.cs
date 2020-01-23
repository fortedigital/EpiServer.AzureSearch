using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class SearchEventHandler<T> where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        private readonly IContentLoader _contentLoader;

        public SearchEventHandler(IAzureSearchService azureSearchService, IContentDocumentBuilder<T> contentDocumentBuilder, IContentLoader contentLoader)
        {
            _azureSearchService = azureSearchService;
            _contentDocumentBuilder = contentDocumentBuilder;
            _contentLoader = contentLoader;
        }

        public void OnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            if (!contentEventArgs.Content.ShouldIndex())
            {
                return;
            }
            
            var document = _contentDocumentBuilder.Build(contentEventArgs.Content);
            Task.Run(() => _azureSearchService.IndexAsync(document));
        }

        public void OnMovedContent(object sender, ContentEventArgs contentEventArgs)
        {
            if (contentEventArgs.TargetLink == ContentReference.WasteBasket)
            {
                DeleteTreeFromIndex(contentEventArgs.Content);
            }
            else
            {
                UpdateTreeInIndex(contentEventArgs.Content);
            }
        }

        private void DeleteTreeFromIndex(IContent root)
        {
            var documentsToIndex = GetDocumentsToReindex(root, true);
            Task.Run(() => _azureSearchService.DeleteAsync(documentsToIndex.ToArray()));
        }

        private void UpdateTreeInIndex(IContent root)
        {
            var documentsToIndex = GetDocumentsToReindex(root);
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }

        private IEnumerable<T> GetDocumentsToReindex(IContent root, bool includeDeleted = false)
        {
            var listResult = new List<T>();
            listResult.AddRange(_contentLoader.GetAllLanguageVersions(root.ContentLink).Where(c => (includeDeleted && c.IsDeleted) || c.ShouldIndex()).Select(_contentDocumentBuilder.Build));
            
            var descendants = _contentLoader.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                listResult.AddRange( _contentLoader.GetAllLanguageVersions(descendant).Where(c => (includeDeleted && c.IsDeleted) || c.ShouldIndex()).Select(_contentDocumentBuilder.Build));
            }
            
            return listResult;
        }
    }
}
