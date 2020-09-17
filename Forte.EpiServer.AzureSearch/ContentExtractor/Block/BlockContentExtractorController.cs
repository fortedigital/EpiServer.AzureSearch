using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor.Block
{
    public class BlockContentExtractorController : IBlockContentExtractorController
    {
        private readonly IEnumerable<IBlockContentExtractor> _extractors;

        public BlockContentExtractorController(IEnumerable<IBlockContentExtractor> extractors)
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

            var extractionResults = _extractors.GetExtractionResults(content);
            
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
