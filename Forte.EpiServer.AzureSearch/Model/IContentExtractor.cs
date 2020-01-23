using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public interface IContentExtractor
    {
        bool CanExtract(IContent content);
        ContentExtractionResult Extract(IContent content);
    }
}
