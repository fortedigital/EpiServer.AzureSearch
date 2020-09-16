using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor.Block;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class SearchableContentExtractor : ISearchableContentExtractor
    {
        public bool CanExtract(IContent content)
        {
            return true;
        }

        public ContentExtractionResult Extract(IContent content)
        {
            var stringValues = new List<string>();

            var properties = content.GetSearchableProperties();

            foreach (var property in properties)
            {
                if (property.Value is XhtmlString xhtmlString)
                {
                    stringValues.Add(xhtmlString.GetPlainTextContent());
                }
                else if (property.Value is ContentArea contentArea)
                {
                    stringValues.Add(contentArea.GetPlainTextContent());
                }
                else if (property.Value is ContentReference contentReference)
                {
                    var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                    var propertyContent = contentLoader.Get<IContent>(contentReference);

                    if (propertyContent is BlockData == false)
                    {
                        continue;
                    }
                    var blockExtractors = ServiceLocator.Current.GetInstance<IEnumerable<IBlockContentExtractor>>();
                    var blockExtractorsController = new BlockContentExtractorController(blockExtractors);
                    var propertyContentExtractionResult = blockExtractorsController.Extract(propertyContent);
                    var text = string.Join(" ", propertyContentExtractionResult);
                    stringValues.Add(text);
                }
                else if (property.Value != null)
                {
                    stringValues.Add(property.Value.ToString());
                }
            }

            return new ContentExtractionResult(stringValues, null);
        }
    }
}
