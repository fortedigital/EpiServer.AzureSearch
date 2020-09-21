using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class BlockSearchEventHandler<T> where T : ContentDocument
    {
        private readonly IContentSoftLinkRepository _linkRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;

        public BlockSearchEventHandler(IContentSoftLinkRepository linkRepository, IContentRepository contentRepository, IContentDocumentBuilder<T> contentDocumentBuilder)
        {
            _linkRepository = linkRepository;
            _contentRepository = contentRepository;
            _contentDocumentBuilder = contentDocumentBuilder;
        }

        public IEnumerable<T> GetBlockContentDocuments(ContentReference contentLink)
        {
            var contentInAllLanguages = _contentRepository.GetAllLanguageVersions(contentLink);
            var documents = contentInAllLanguages
                .Select(_contentDocumentBuilder.Build)
                .ToList();
            return documents;
        }
        
        public IEnumerable<ContentReference> GetBlockParentPages(ContentReference contentLink)
        {
            var parents = new List<ContentReference>();
            
            AddParentsFromContentReferences(contentLink, parents);
            AddParentsFromXhtmlProperties(contentLink, parents);

            return parents.DistinctBy(parent => parent.ID);
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
                var parentContentLink = HasContentParent(softLink) ? softLink.OwnerContentLink.ToReferenceWithoutVersion() : null;
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

        private static bool HasContentParent(SoftLink link)
        {
            return link.SoftLinkType == ReferenceType.PageLinkReference &&
                   !ContentReference.IsNullOrEmpty(link.OwnerContentLink);
        }

        public IEnumerable<T> GetDocumentsToReindex(IContent root)
        {
            var listResult = new List<T>();

            var parents = GetBlockParentPages(root.ContentLink)
                .SelectMany(GetBlockContentDocuments);
            listResult.AddRange(parents);

            return listResult;
        }
    }
}
