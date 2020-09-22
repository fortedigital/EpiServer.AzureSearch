using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class SearchEventHandler<T> where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentLoader _contentLoader;
        private readonly PageSearchEventHandler<T> _pageSearchEventHandler;
        private readonly BlockSearchEventHandler<T> _blockSearchEventHandler;

        public SearchEventHandler(IAzureSearchService azureSearchService, IContentLoader contentLoader,
            PageSearchEventHandler<T> pageSearchEventHandler, BlockSearchEventHandler<T> blockSearchEventHandler)
        {
            _azureSearchService = azureSearchService;
            _contentLoader = contentLoader;
            _pageSearchEventHandler = pageSearchEventHandler;
            _blockSearchEventHandler = blockSearchEventHandler;
        }

        public void OnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var documentsToIndex = new List<T>();
            var content = contentEventArgs.Content;
            switch (content)
            {
                case PageData _:
                    var pageDocuments = !content.ShouldIndexPage()
                        ? Enumerable.Empty<T>()
                        : _pageSearchEventHandler.GetPageContentDocuments(content.ContentLink);
                    documentsToIndex.AddRange(pageDocuments);
                    break;
                
                case BlockData _:
                    var blockParentPagesLinks = _blockSearchEventHandler.GetBlockParentPages(content.ContentLink);
                    foreach (var blockParentPageLink in blockParentPagesLinks)
                    {
                        var blockParentPageDocuments =
                            _pageSearchEventHandler.GetPageContentDocuments(blockParentPageLink);
                        documentsToIndex.AddRange(blockParentPageDocuments);
                    }
                    break;
            }

            if (documentsToIndex.IsNullOrEmpty() == false)
            {
                Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
            }
        }

        public void OnMovedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            switch (content)
            {
                case PageData _:
                    if (contentEventArgs.TargetLink == ContentReference.WasteBasket)
                    {
                        DeleteDeletedPageTreeFromIndex(content);
                    }
                    else
                    {
                        UpdatePageTreeInIndex(content);
                    }
                    break;
                case BlockData _:
                    if (contentEventArgs.TargetLink == ContentReference.WasteBasket)
                    {
                        UpdateBlockParentPagesInIndex(content);
                    }
                    break;
            }
        }
        
        public void OnSavingContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            var contentPreviousVersion = GetContentPreviousVersion(content.ContentLink);
            if (contentPreviousVersion != null && IsContentMarkedAsExpired(contentEventArgs, contentPreviousVersion))
            {
                switch (content)
                {
                    case PageData _:
                        DeleteExpiredPageFromIndex(content);
                        break;
                    case BlockData _:
                        UpdateBlockParentPagesInIndex(content);
                        break;
                }
            }
        }

        private void DeleteExpiredPageFromIndex(IContent content)
        {
            var documentToRemoveFromIndex = _pageSearchEventHandler.GetPageExpiredContentDocument(content.ContentLink);
            Task.Run(() => _azureSearchService.DeleteAsync(new [] {documentToRemoveFromIndex}));
        }
        private void DeleteDeletedPageTreeFromIndex(IContent root)
        {
            var documentsToRemoveFromIndex = _pageSearchEventHandler.GetDocumentsToReindex(root, true);
            Task.Run(() => _azureSearchService.DeleteAsync(documentsToRemoveFromIndex.ToArray()));
        }
        
        private void UpdatePageTreeInIndex(IContent root)
        {
            var documentsToIndex = _pageSearchEventHandler.GetDocumentsToReindex(root).ToList();
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }
        
        private void UpdateBlockParentPagesInIndex(IContent content)
        {
            var documentsToIndex = _blockSearchEventHandler.GetDocumentsToReindex(content).ToList();

            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }

        private static bool IsContentMarkedAsExpired(ContentEventArgs contentEventArgs, IVersionable pagePreviousVersion)
        {
            return pagePreviousVersion != null && contentEventArgs.Content is PageData page &&
                   pagePreviousVersion.StopPublish != page.StopPublish && page.StopPublish <= DateTime.Now;
        }

        private PageData GetContentPreviousVersion(ContentReference reference)
        {
            if (ContentReference.IsNullOrEmpty(reference))
            {
                return null;
            }
            
            var previousPageReference = reference.ToReferenceWithoutVersion();
            return _contentLoader.TryGet<PageData>(previousPageReference, out var pageData) ? pageData : null;
        }
    }
}
