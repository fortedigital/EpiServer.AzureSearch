using System.Collections.Generic;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.ContentExtractor.Block
{
    public interface IBlockContentExtractorController
    {
        IEnumerable<string> Extract(IContentData content);
    }
}
