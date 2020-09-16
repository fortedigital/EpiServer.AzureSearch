using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
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
            var documentsToIndex = new List<T>();
            var content = contentEventArgs.Content;
            switch (contentEventArgs.Content)
            {
                case PageData _:
                    if (!contentEventArgs.Content.ShouldIndex())
                    {
                        return;
                    }
            
                    var contentInAllLanguageVersions = _contentLoader.GetAllLanguageVersions(content.ContentLink);
                    documentsToIndex = contentInAllLanguageVersions
                        .Select(c => _contentDocumentBuilder.Build(c))
                        .ToList();
            
                    break;
                case BlockData _:
                    var blockParentPagesLinks = GetBlockParentPages(content.ContentLink);
                    foreach (var blockParentPageLink in blockParentPagesLinks)
                    {
                        var blockParentPageInAllLanguageVersions = _contentLoader.GetAllLanguageVersions(blockParentPageLink);
                        var documents = blockParentPageInAllLanguageVersions
                            .Select(c => _contentDocumentBuilder.Build(c))
                            .ToList();
                        documentsToIndex.AddRange(documents);
                    }
                    break;
            }

            if (documentsToIndex.IsNullOrEmpty() == false)
            {
                Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
            }
        }

        private static IEnumerable<ContentReference> GetBlockParentPages(ContentReference contentLink)
        {
            var linkRepository = ServiceLocator.Current.GetInstance<IContentSoftLinkRepository>();
            var softLinks = linkRepository.Load(contentLink, true);
            var parents = new List<ContentReference>();
            foreach (var softLink in softLinks)
            {
                var parentContentLink = HasParent(softLink) ? softLink.OwnerContentLink.ToReferenceWithoutVersion() : null;
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                var content = contentLoader.Get<IContent>(parentContentLink);
                if (content is PageData)
                {
                    parents.Add(parentContentLink);
                }
                else
                {
                    parents.AddRange(GetBlockParentPages(parentContentLink));
                }
            }

            return parents;
        }

        private static bool HasParent(SoftLink link)
        {
            return link.SoftLinkType == ReferenceType.PageLinkReference &&
                   !ContentReference.IsNullOrEmpty(link.OwnerContentLink);
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
        
        public void OnSavingContent(object sender, ContentEventArgs contentEventArgs)
        {
            var content = contentEventArgs.Content;
            var pagePreviousVersion = GetPagePreviousVersion(content.ContentLink);
            if (pagePreviousVersion != null && IsContentMarkedAsExpired(contentEventArgs, pagePreviousVersion))
            {
                DeleteTreeFromIndex(content);
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
        
        private static bool IsContentMarkedAsExpired(ContentEventArgs contentEventArgs, IVersionable pagePreviousVersion)
        {
            return pagePreviousVersion != null && contentEventArgs.Content is PageData page &&
                   pagePreviousVersion.StopPublish != page.StopPublish && page.StopPublish <= DateTime.Now;
        }

        private PageData GetPagePreviousVersion(ContentReference reference)
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
