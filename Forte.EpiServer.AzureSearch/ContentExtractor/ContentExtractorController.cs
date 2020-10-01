using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
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

        public IEnumerable<string> ExtractPage(IContent content)
        {
            var results = ExtractPageInternal(content, new HashSet<ContentReference>(), this);
            return FlattenResults(results);
        }
        
        public string ExtractBlock(IContentData content)
        {
            var results = ExtractBlockInternal(content, this);
            var texts = FlattenResults(results);
            return string.Join(BlockExtractedTextFragmentsSeparator, texts);
        }

        private static IEnumerable<string> FlattenResults(IEnumerable<ContentExtractionResult> results)
        {
            return results
                .SelectMany(r => r.Values)
                .Where(v => string.IsNullOrEmpty(v) == false)
                .Distinct();
        }

        private IEnumerable<ContentExtractionResult> ExtractPageInternal(IContent content,
            ISet<ContentReference> visitedContent, ContentExtractorController extractor)
        {
            if (visitedContent.Contains(content.ContentLink))
            {
                return Enumerable.Empty<ContentExtractionResult>();
            }

            visitedContent.Add(content.ContentLink);

            if (content.IsDeleted)
            {
                return Enumerable.Empty<ContentExtractionResult>();
            }

            var results = new List<ContentExtractionResult>();
            var extractionResults = GetExtractionResults(_extractors, content, extractor);
            results.AddRange(extractionResults);

            var relatedContentReferences = extractionResults.SelectMany(r => r.ContentReferences);

            foreach (var relatedContentReference in relatedContentReferences)
            {
                results.AddRange(ExtractPageInternal(relatedContentReference, visitedContent, extractor));
            }
            
            return results;
        }

        private IEnumerable<ContentExtractionResult> ExtractBlockInternal(IContentData content, IContentExtractorController extractor)
        {
            var result = new List<ContentExtractionResult>();
            
            var extractionResults = GetExtractionResults(_extractors, content, extractor);
            result.AddRange(extractionResults);

            var relatedContentList = extractionResults.SelectMany(r => r.ContentReferences);
            foreach (var relatedContent in relatedContentList)
            {
                result.AddRange(ExtractBlockInternal(relatedContent, extractor));
            }
            
            return result;
        }

        private static List<ContentExtractionResult> GetExtractionResults(IEnumerable<IContentExtractor> contentExtractors, IContentData content,
            IContentExtractorController extractor)
        {
            return contentExtractors
                .Where(e => e.CanExtract(content))
                .Select(e => e.Extract(content, extractor))
                .ToList();
        }
    }
}
