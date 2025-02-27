using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Plugin.Filters;

namespace Forte.EpiServer.AzureSearch.Events
{
    public class BlockDocumentsProvider<T> where T : ContentDocument
    {
        private readonly IContentSoftLinkRepository _linkRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IContentDocumentBuilder<T> _contentDocumentBuilder;
        private readonly IEnumerable<IContentIndexFilter> _contentIndexFilters;

        public BlockDocumentsProvider(IContentSoftLinkRepository linkRepository, IContentRepository contentRepository, IContentDocumentBuilder<T> contentDocumentBuilder,
            IEnumerable<IContentIndexFilter> contentIndexFilters)
        {
            _linkRepository = linkRepository;
            _contentRepository = contentRepository;
            _contentDocumentBuilder = contentDocumentBuilder;
            _contentIndexFilters = contentIndexFilters;
        }

        public IReadOnlyCollection<T> GetDocuments(IContent block)
        {
            var documents = block.IsMasterLanguageBranch()
                ? GetParentPagesAllLanguagesDocuments(block)
                : GetParentPagesSpecificLanguageDocuments(block);

            return documents.ToList();
        }

        private IEnumerable<ContentReference> GetBlockParentPagesInBlockLanguage(ContentReference contentLink)
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
                    parents.AddRange(GetBlockParentPagesInBlockLanguage(parentContentLink));
                }
            }
        }

        private static bool HasContentParent(SoftLink link)
        {
            return link.SoftLinkType == ReferenceType.PageLinkReference &&
                   !ContentReference.IsNullOrEmpty(link.OwnerContentLink);
        }

        private IEnumerable<T> GetParentPagesAllLanguagesDocuments(IContent root)
        {
            var listResult = new List<T>();

            var parents = GetBlockParentPagesInBlockLanguage(root.ContentLink)
                .SelectMany(GetPageAllLanguageContentDocuments);

            listResult.AddRange(parents);

            return listResult;
        }

        private IEnumerable<T> GetPageAllLanguageContentDocuments(ContentReference contentLink)
        {
            var contentInAllLanguages = _contentRepository.GetAllLanguageVersions(contentLink);

            var documents = contentInAllLanguages
                .Where(content => _contentIndexFilters.All(filter => filter.ShouldIndexContent(content)))
                .Select(_contentDocumentBuilder.Build)
                .ToList();

            return documents;
        }

        private IEnumerable<T> GetParentPagesSpecificLanguageDocuments(IContent root)
        {
            var listResult = new List<T>();

            var parents = GetBlockParentPagesInBlockLanguage(root.ContentLink)
                .Select(contentLink => _contentRepository.Get<IContent>(contentLink))
                .Where(content => _contentIndexFilters.All(filter => filter.ShouldIndexContent(content)))
                .Select(_contentDocumentBuilder.Build);

            listResult.AddRange(parents);

            return listResult;
        }
    }
}
