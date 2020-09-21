using EPiServer.Core;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor.Block;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class ContentDataExtensions
    {
        public static string ExtractTextFromBlock(this IContentData blockContent)
        {
            const string blockExtractedTextSeparator = " ";
            var blockExtractorsController = ServiceLocator.Current.GetInstance<IBlockContentExtractorController>();
            var propertyContentExtractionResult = blockExtractorsController.Extract(blockContent);
            var text = string.Join(blockExtractedTextSeparator, propertyContentExtractionResult);
            return text;
        }
    }
}
