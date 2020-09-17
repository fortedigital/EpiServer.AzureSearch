using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.ContentExtractor;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class ContentExtractorExtensions
    {
        public static List<ContentExtractionResult> GetExtractionResults(this IEnumerable<IContentExtractor> contentExtractors, IContentData content)
        {
            return contentExtractors
                .Where(e => e.CanExtract(content))
                .Select(e => e.Extract(content))
                .ToList();
        }
    }
}
