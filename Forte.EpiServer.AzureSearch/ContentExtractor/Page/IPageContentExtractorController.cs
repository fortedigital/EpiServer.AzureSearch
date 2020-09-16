using System.Collections.Generic;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.ContentExtractor.Page
{
    public interface IPageContentExtractorController
    {
        IEnumerable<string> Extract(IContent content);
    }
}
