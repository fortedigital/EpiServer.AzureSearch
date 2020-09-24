using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class IndexableContentExtractor : IContentExtractor
    {
        private readonly IContentLoader _contentLoader;

        public IndexableContentExtractor(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }
        public bool CanExtract(IContentData content)
        {
            return true;
        }

        public ContentExtractionResult Extract(IContentData content, ContentExtractorController extractor)
        {
            var stringValues = new List<string>();

            var properties = content.GetIndexableProperties();

            foreach (var property in properties)
            {
                switch (property.Value)
                {
                    case XhtmlString xhtmlString:
                        stringValues.Add(XhtmlStringExtractor.GetPlainTextContent(xhtmlString, extractor));
                        break;
                    case BlockData localBlock:
                        stringValues.AddRange(extractor.Extract(localBlock));
                        break;
                    //TODO: check if this case is used ORM blockData
                    case ContentReference contentReference:
                    {
                        var propertyContent = _contentLoader.Get<IContent>(contentReference);

                        if (propertyContent is BlockData == false)
                        {
                            continue;
                        }
                        stringValues.AddRange(extractor.Extract(propertyContent));
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
    }
}
