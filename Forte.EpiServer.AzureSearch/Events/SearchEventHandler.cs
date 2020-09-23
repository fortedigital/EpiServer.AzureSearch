using System;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class SearchEventHandler<T> where T : ContentDocument
    {
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentLoader _contentLoader;
        private readonly PageSearchEventHandler<T> _pageSearchEventHandler;
        private readonly BlockSearchEventHandler<T> _blockSearchEventHandler;
        private readonly IUrlResolver _urlResolver;

        public SearchEventHandler(IAzureSearchService azureSearchService, IContentLoader contentLoader,
            PageSearchEventHandler<T> pageSearchEventHandler, BlockSearchEventHandler<T> blockSearchEventHandler,
            IUrlResolver urlResolver)
        {
            _azureSearchService = azureSearchService;
            _contentLoader = contentLoader;
            _pageSearchEventHandler = pageSearchEventHandler;
            _blockSearchEventHandler = blockSearchEventHandler;
            _urlResolver = urlResolver;
        }

        //Event handler for keeping 'OldUrl' in contentEventArgs
        public void OnPublishingContent(object sender, ContentEventArgs contentEventArgs)
        {
            if (contentEventArgs.Content is PageData == false)
            {
                return;
            }
            var oldUrl = _urlResolver.GetUrl(new ContentReference(contentEventArgs.Content.ContentLink.ID));

            if (string.IsNullOrEmpty(oldUrl) == false)
            {
                contentEventArgs.Items.Add(PageSearchEventHandler<T>.OldUrlKey, oldUrl);
            }
        }
        
        //Event handler for publishing page and block content
        public void OnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;

            switch (content)
            {
                case PageData _:
                    var documentsToIndex = _pageSearchEventHandler.GetDocuments(content, contentEventArgs);
                    Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
                    break;

                case BlockData _:
                    UpdateBlockParentPagesInIndex(content);
                    break;
            }
        }

        //Event handler for moving pages and moving pages and blocks to WasteBasket
        public void OnMovedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            switch (content)
            {
                case PageData _:
                    if (contentEventArgs.TargetLink == ContentReference.WasteBasket)
                    {
                        DeletePageTreeFromIndex(content);
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
        
        //Event handler for deleting index for expired pages and updating index of pages that use expired blocks 
        public void OnSavingContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            var contentPreviousVersion = GetContentPreviousVersion(content.ContentLink);
            if (contentPreviousVersion != null && IsContentMarkedAsExpired(contentEventArgs, contentPreviousVersion))
            {
                switch (content)
                {
                    case PageData _:
                        DeletePageFromIndex(content);
                        break;
                    case BlockData _:
                        UpdateBlockParentPagesInIndex(content);
                        break;
                }
            }
        }
        
        //Event handler for deleting page index(for pages) and updating page parents(for blocks) during deleting of a specific language content version
        public void OnDeletingContentLanguage(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            switch (content)
            {
                case PageData _:
                    DeletePageFromIndex(content);
                    break;
                case BlockData _:
                    UpdateBlockParentPagesInIndex(content);
                    break;
            }
        }

        private void DeletePageFromIndex(IContent content)
        {
            var documentToRemoveFromIndex = _pageSearchEventHandler.GetPageVersionContentDocument(content.ContentLink);
            Task.Run(() => _azureSearchService.DeleteAsync(documentToRemoveFromIndex));
        }
        private void DeletePageTreeFromIndex(IContent root)
        {
            var documentsToRemoveFromIndex = _pageSearchEventHandler.GetPageTreeAllLanguagesDocuments(root, true);
            Task.Run(() => _azureSearchService.DeleteAsync(documentsToRemoveFromIndex.ToArray()));
        }
        
        private void UpdatePageTreeInIndex(IContent root)
        {
            var documentsToIndex = root.IsMasterLanguageBranch()
                ? _pageSearchEventHandler.GetPageTreeAllLanguagesDocuments(root)
                : _pageSearchEventHandler.GetPageTreeSpecificLanguageDocuments(root);
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }
        
        private void UpdateBlockParentPagesInIndex(IContent content)
        {
            var documentsToIndex = _blockSearchEventHandler.GetDocuments(content);
            
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
