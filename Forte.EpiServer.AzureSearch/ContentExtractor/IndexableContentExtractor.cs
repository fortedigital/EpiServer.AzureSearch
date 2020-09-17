using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor.Block;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class IndexableContentExtractor : IIndexableContentExtractor
    {
        public bool CanExtract(IContent content)
        {
            return true;
        }
        public bool CanExtract(object content)
        {
            return true;
        }

        public ContentExtractionResult Extract(IContentData content)
        {
            var stringValues = new List<string>();

            var properties = content.GetIndexableProperties();

            foreach (var property in properties)
            {
                switch (property.Value)
                {
                    case XhtmlString xhtmlString:
                        stringValues.Add(xhtmlString.GetPlainTextContent());
                        break;
                    case BlockData localBlock:
                        stringValues.Add(ExtractTextFromBlock(localBlock));
                        break;
                    case ContentReference contentReference:
                    {
                        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                        var propertyContent = contentLoader.Get<IContent>(contentReference);

                        if (propertyContent is BlockData == false)
                        {
                            continue;
                        }
                        stringValues.Add(ExtractTextFromBlock(propertyContent));
                        break;
                    }
                    default:
                    {
                        if (property.Value != null)
                        {
                            stringValues.Add(property.Value.ToString());
                        }

                        break;
                    }
                }
            }

            return new ContentExtractionResult(stringValues, null);
        }

        private static string ExtractTextFromBlock(IContentData blockContent)
        {
            var blockExtractors = ServiceLocator.Current.GetInstance<IEnumerable<IBlockContentExtractor>>();
            var blockExtractorsController = new BlockContentExtractorController(blockExtractors);
            var propertyContentExtractionResult = blockExtractorsController.Extract(blockContent);
            var text = string.Join(" ", propertyContentExtractionResult);
            return text;
        }
    }
}
