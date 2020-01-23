using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class ContentExtractorController : IContentExtractorController
    {
        private readonly IEnumerable<IContentExtractor> _extractors;

        public ContentExtractorController(IEnumerable<IContentExtractor> extractors)
        {
            _extractors = extractors;
        }

        public IEnumerable<string> Extract(IContent content)
        {
            return ExtractInternal(content, new HashSet<ContentReference>())
                .SelectMany(r => r.Values)
                .Where(v => string.IsNullOrEmpty(v) == false)
                .Distinct();
        }

        private IEnumerable<ContentExtractionResult> ExtractInternal(IContent content, ISet<ContentReference> visitedContent)
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

            var extractionResults = _extractors.Where(e => e.CanExtract(content))
                .Select(e => e.Extract(content))
                .ToList();
            
            result.AddRange(extractionResults);

            var relatedContentList = extractionResults.SelectMany(r => r.ContentReferences);

            foreach (var relatedContent in relatedContentList)
            {
                result.AddRange(ExtractInternal(relatedContent, visitedContent));
            }
            
            return result;
        }
    }
}
