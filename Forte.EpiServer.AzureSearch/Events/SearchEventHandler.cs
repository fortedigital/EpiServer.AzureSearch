using System;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web.Routing;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class SearchEventHandler<T> where T : ContentDocument
    {
        private const string IsContentMovedFromWasteBasketKey = "IsContentMovedFromWasteBasket";
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentLoader _contentLoader;
        private readonly PageDocumentsProvider<T> _pageDocumentsProvider;
        private readonly BlockDocumentsProvider<T> _blockDocumentsProvider;
        private readonly IUrlResolver _urlResolver;

        public SearchEventHandler(IAzureSearchService azureSearchService, IContentLoader contentLoader,
            PageDocumentsProvider<T> pageDocumentsProvider, BlockDocumentsProvider<T> blockDocumentsProvider,
            IUrlResolver urlResolver)
        {
            _azureSearchService = azureSearchService;
            _contentLoader = contentLoader;
            _pageDocumentsProvider = pageDocumentsProvider;
            _blockDocumentsProvider = blockDocumentsProvider;
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
                contentEventArgs.Items.Add(PageDocumentsProvider<T>.OldUrlKey, oldUrl);
            }
        }
        
        //Event handler for publishing page and block content
        public void OnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;

            switch (content)
            {
                case PageData _:
                    UpdateIndexAfterPagePublish(content, contentEventArgs);
                    break;

                case BlockData _:
                    UpdateBlockParentPagesInIndex(content);
                    break;
            }
        }

        //Event handler for checking if content is moved back from the WasteBasket
        public void OnMovingContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            if (content.IsDeleted)
            {
                contentEventArgs.Items.Add(IsContentMovedFromWasteBasketKey, true);
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

                    var isContentMovedFromWasteBasket = contentEventArgs.Items[IsContentMovedFromWasteBasketKey];
                    if (isContentMovedFromWasteBasket != null && (bool)isContentMovedFromWasteBasket)
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
        
        //Event handler for page access rights change
        public void OnContentSecuritySaved(object sender, ContentSecurityEventArg contentSecurityEventArgs)
        {
            var contentLink = contentSecurityEventArgs.ContentLink;
            var content = _contentLoader.Get<IContent>(contentLink);

            if (content is PageData)
            {
                UpdateIndexAfterPageAccessRightsChange(content);
            }
        }
        
        private void UpdateIndexAfterPageAccessRightsChange(IContent content)
        {
            var documentsToIndex = _pageDocumentsProvider.GetPageTreeDocuments(content, true);
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
        }
        
        private void UpdateIndexAfterPagePublish(IContent content, ContentEventArgs contentEventArgs)
        {
            var documentsToIndex = _pageDocumentsProvider.GetDocuments(content, contentEventArgs);
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
        }
        
        private void UpdateBlockParentPagesInIndex(IContent content)
        {
            var documentsToIndex = _blockDocumentsProvider.GetDocuments(content);
            
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }
        
        private void DeletePageTreeFromIndex(IContent root)
        {
            var documentsToRemoveFromIndex = _pageDocumentsProvider.GetPageTreeAllLanguagesDocuments(root, true);
            Task.Run(() => _azureSearchService.DeleteAsync(documentsToRemoveFromIndex.ToArray()));
        }
        
        private void UpdatePageTreeInIndex(IContent root)
        {
            var documentsToIndex = _pageDocumentsProvider.GetPageTreeDocuments(root, root.IsMasterLanguageBranch());
            Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex.ToArray()));
        }

        private void DeletePageFromIndex(IContent content)
        {
            var documentToRemoveFromIndex = _pageDocumentsProvider.GetPageVersionContentDocument(content.ContentLink);
            Task.Run(() => _azureSearchService.DeleteAsync(documentToRemoveFromIndex));
        }

        private static bool IsContentMarkedAsExpired(ContentEventArgs contentEventArgs, IContent contentPreviousVersion)
        {
            if (!(contentPreviousVersion is IVersionable contentPreviousVersionInfo))
            {
                return false;
            }
            
            return contentEventArgs.Content is IVersionable content &&
                   contentPreviousVersionInfo.StopPublish != content.StopPublish &&
                   content.StopPublish <= DateTime.Now;
        }

        private IContent GetContentPreviousVersion(ContentReference reference)
        {
            if (ContentReference.IsNullOrEmpty(reference))
            {
                return null;
            }
            
            var previousVersionReference = reference.ToReferenceWithoutVersion();
            return _contentLoader.TryGet<IContent>(previousVersionReference, out var content) ? content : null;
        }
    }
}
