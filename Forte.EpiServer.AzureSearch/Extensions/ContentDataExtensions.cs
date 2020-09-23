using EPiServer.Core;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor.Block;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public class BlockContentExtractor
    {
        private readonly IBlockContentExtractorController _controller;

        public BlockContentExtractor(IBlockContentExtractorController controller)
        {
            _controller = controller;
        }
        public string ExtractTextFromBlock(IContentData blockContent)
        {
            var propertyContentExtractionResult = _controller.Extract(blockContent);
            var text = string.Join(BlockContentExtractorController.BlockExtractedTextFragmentsSeparator, propertyContentExtractionResult);
            return text;
        }
    }
}
