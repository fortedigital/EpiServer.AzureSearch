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

        public IEnumerable<string> Extract(IContentData content)
        {
            return ExtractInternal(content)
                .SelectMany(r => r.Values)
                .Where(v => string.IsNullOrEmpty(v) == false)
                .Distinct();
        }

        private IEnumerable<ContentExtractionResult> ExtractInternal(IContentData content)
        {
            var result = new List<ContentExtractionResult>();
            
            var extractionResults = _extractors.GetExtractionResults(content);
            
            result.AddRange(extractionResults);

            var relatedContentList = extractionResults.SelectMany(r => r.ContentReferences);

            foreach (var relatedContent in relatedContentList)
            {
                result.AddRange(ExtractInternal(relatedContent));
            }
            
            return result;
        }
    }
}
