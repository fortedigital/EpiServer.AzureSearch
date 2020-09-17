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
        private readonly IContentRepository _contentRepository;
        private readonly IContentSoftLinkRepository _linkRepository;


        public SearchEventHandler(IAzureSearchService azureSearchService, IContentDocumentBuilder<T> contentDocumentBuilder,
            IContentRepository contentRepository, IContentSoftLinkRepository linkRepository)
        {
            _azureSearchService = azureSearchService;
            _contentDocumentBuilder = contentDocumentBuilder;
            _contentRepository = contentRepository;
            _linkRepository = linkRepository;
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

                    documentsToIndex.AddRange(GetContentDocuments(content.ContentLink));
            
                    break;
                case BlockData _:
                    var blockParentPagesLinks = GetBlockParentPages(content.ContentLink);
                    foreach (var blockParentPageLink in blockParentPagesLinks)
                    {
                        documentsToIndex.AddRange(GetContentDocuments(blockParentPageLink));
                    }
                    break;
            }

            if (documentsToIndex.IsNullOrEmpty() == false)
            {
                Task.Run(() => _azureSearchService.IndexAsync(documentsToIndex));
            }
        }

        private IEnumerable<T> GetContentDocuments(ContentReference contentLink)
        {
            var blockParentPageInAllLanguageVersions = _contentRepository.GetAllLanguageVersions(contentLink);
            var documents = blockParentPageInAllLanguageVersions
                .Select(c => _contentDocumentBuilder.Build(c))
                .ToList();
            return documents;
        }

        private IEnumerable<ContentReference> GetBlockParentPages(ContentReference contentLink)
        {
            var parents = new List<ContentReference>();
            
            AddParentsFromContentReferences(contentLink, parents);
            AddParentsFromXhtmlProperties(contentLink, parents);

            return parents;
        }

        private void AddParentsFromContentReferences(ContentReference contentLink, List<ContentReference> parents)
        {
            var referencesToContent = _contentRepository.GetReferencesToContent(contentLink, false);
            parents.AddRange(referencesToContent.Select(rtc => rtc.OwnerID));
        }
        
        private void AddParentsFromXhtmlProperties(ContentReference contentLink, List<ContentReference> parents)
        {
            var softLinks = _linkRepository.Load(contentLink, true);
            foreach (var softLink in softLinks)
            {
                var parentContentLink = HasParent(softLink) ? softLink.OwnerContentLink.ToReferenceWithoutVersion() : null;
                var content = _contentRepository.Get<IContent>(parentContentLink);
                if (content is PageData)
                {
                    parents.Add(parentContentLink);
                }
                else
                {
                    parents.AddRange(GetBlockParentPages(parentContentLink));
                }
            }
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
            listResult.AddRange(_contentRepository.GetAllLanguageVersions(root.ContentLink).Where(c => (includeDeleted && c.IsDeleted) || c.ShouldIndex()).Select(_contentDocumentBuilder.Build));
            
            var descendants = _contentRepository.GetDescendents(root.ContentLink);
            foreach (var descendant in descendants)
            {
                listResult.AddRange( _contentRepository.GetAllLanguageVersions(descendant).Where(c => (includeDeleted && c.IsDeleted) || c.ShouldIndex()).Select(_contentDocumentBuilder.Build));
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
            return _contentRepository.TryGet<PageData>(previousPageReference, out var pageData) ? pageData : null;
        }
    }
}
