using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class ContentExtractorController : IContentExtractorController
    {
        public const string BlockExtractedTextFragmentsSeparator = " ";
        private readonly IEnumerable<IContentExtractor> _extractors;

        public ContentExtractorController(IEnumerable<IContentExtractor> contentExtractors)
        {
            _extractors = contentExtractors;
        }

        public IEnumerable<string> Extract(IContent content)
        {
            return ExtractInternal(content, new HashSet<ContentReference>(), this)
                .SelectMany(r => r.Values)
                .Where(v => string.IsNullOrEmpty(v) == false)
                .Distinct();
        }
        
        public IEnumerable<string> Extract(IContentData content)
        {
            return ExtractInternal(content, this)
                .SelectMany(r => r.Values)
                .Where(v => string.IsNullOrEmpty(v) == false)
                .Distinct();
        }

        private IEnumerable<ContentExtractionResult> ExtractInternal(IContent content,
            ISet<ContentReference> visitedContent, ContentExtractorController extractor)
        {
            var result = new List<ContentExtractionResult>();

            if (visitedContent.Contains(content.ContentLink))
            {
                return result;
            }

            visitedContent.Add(content.ContentLink);

            if (content.IsDeleted)
            {
                return Enumerable.Empty<ContentExtractionResult>();
            }

            var extractionResults = _extractors.GetExtractionResults(content, extractor);

            result.AddRange(extractionResults);

            var relatedContentList = extractionResults.SelectMany(r => r.ContentReferences);

            foreach (var relatedContent in relatedContentList)
            {
                result.AddRange(ExtractInternal(relatedContent, visitedContent, extractor));
            }
            
            return result;
        }

        private IEnumerable<ContentExtractionResult> ExtractInternal(IContentData content, ContentExtractorController extractor)
        {
            var result = new List<ContentExtractionResult>();
            
            var extractionResults = _extractors.GetExtractionResults(content, extractor);
            result.AddRange(extractionResults);

            var relatedContentList = extractionResults.SelectMany(r => r.ContentReferences);
            foreach (var relatedContent in relatedContentList)
            {
                result.AddRange(ExtractInternal(relatedContent, extractor));
            }
            
            return result;
        }
    }
}
