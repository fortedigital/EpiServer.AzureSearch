using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Plugin.Filters;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class ContentIndexer<T> : IContentIndexer
        where T : ContentDocument
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAzureSearchService _azureSearchService;
        private readonly IContentDocumentBuilder<T> _documentBuilder;
        private readonly IEnumerable<IContentIndexFilter> _contentIndexFilters;

        public ContentIndexer(
            IContentLoader contentLoader,
            IAzureSearchService azureSearchService,
            IContentDocumentBuilder<T> documentBuilder,
            IEnumerable<IContentIndexFilter> contentIndexFilters)
        {
            _contentLoader = contentLoader;
            _azureSearchService = azureSearchService;
            _documentBuilder = documentBuilder;
            _contentIndexFilters = contentIndexFilters;
        }

        public async Task Index(ContentReference rootContentLink, IndexContentRequest indexContentRequest)
        {
            if (indexContentRequest.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (indexContentRequest.IgnoreContent.Contains(rootContentLink))
            {
                return;
            }

            if (indexContentRequest.VisitedContent.Contains(rootContentLink))
            {
                return;
            }

            if (indexContentRequest.Statistics.Exceptions.Count >= indexContentRequest.ExceptionThreshold)
            {
                return;
            }

            try
            {
                await IndexLanguageVersions(rootContentLink, indexContentRequest);
                indexContentRequest.VisitedContent.Add(rootContentLink);
            }
            catch (Exception e)
            {
                indexContentRequest.Statistics.FailedContentReferences.Add(rootContentLink);
                indexContentRequest.Statistics.Exceptions.Add(e);
            }

            var children = _contentLoader.GetChildren<PageData>(rootContentLink);

            foreach (var child in children)
            {
                await Index(child.ContentLink, indexContentRequest);
            }
        }

        private async Task IndexLanguageVersions(ContentReference contentReference, IndexContentRequest indexContentRequest)
        {
            foreach (var languageVersion in _contentLoader.GetAllLanguageVersions(contentReference)
                         .Where(content => _contentIndexFilters.All(filter => filter.ShouldIndexContent(content))))
            {
                indexContentRequest.OnStatusChanged($"Indexing content: Name: {languageVersion.Name}, ContentLinkId: {languageVersion.ContentLink.ID}");
                var contentDocument = _documentBuilder.Build(languageVersion);
                await _azureSearchService.IndexAsync(contentDocument);
            }
        }
    }
}
