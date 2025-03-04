using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class ContentExtractionResult
    {
        public IEnumerable<string> Values { get; }

        public IEnumerable<IContent> ContentReferences { get; }

        public ContentExtractionResult(IEnumerable<string> values, IEnumerable<IContent> contentReferences)
        {
            Values = values ?? Enumerable.Empty<string>();
            ContentReferences = contentReferences ?? Enumerable.Empty<IContent>();
        }
    }
}
