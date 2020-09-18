using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public interface IContentExtractor
    {
        bool CanExtract(IContentData content);
        ContentExtractionResult Extract(IContentData content);
    }
}
