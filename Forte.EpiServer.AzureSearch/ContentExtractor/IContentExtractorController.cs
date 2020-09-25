using System.Collections.Generic;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public interface IContentExtractorController
    {
        IEnumerable<string> ExtractPage(IContent content);
        string ExtractBlock(IContentData content);
    }
}
