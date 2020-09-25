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
        private readonly XhtmlStringExtractor _xhtmlStringExtractor;

        public IndexableContentExtractor(IContentLoader contentLoader, XhtmlStringExtractor xhtmlStringExtractor)
        {
            _contentLoader = contentLoader;
            _xhtmlStringExtractor = xhtmlStringExtractor;
        }
        public bool CanExtract(IContentData content)
        {
            return true;
        }

        public ContentExtractionResult Extract(IContentData content, ContentExtractorController extractor)
        {
            var texts = new List<string>();

            var properties = content.GetIndexableProperties();

            foreach (var property in properties)
            {
                var textsFromProperty = ExtractTextFromProperty(extractor, property);
                if (string.IsNullOrEmpty(textsFromProperty) == false)
                {
                    texts.Add(textsFromProperty);
                }
            }

            return new ContentExtractionResult(texts, null);
        }

        private string ExtractTextFromProperty(ContentExtractorController extractor, PropertyData property)
        {
            switch (property.Value)
            {
                case XhtmlString xhtmlString:
                    return _xhtmlStringExtractor.GetPlainTextContent(xhtmlString, extractor);
                case BlockData localBlock:
                    return extractor.ExtractBlock(localBlock);
                case ContentReference contentReference:
                {
                    var propertyContent = _contentLoader.Get<IContent>(contentReference);

                    return propertyContent is BlockData 
                        ? extractor.ExtractBlock(propertyContent)
                        : string.Empty;
                }
                default:
                {
                    if (property.Value == null)
                    {
                        return string.Empty;
                    }
                    var text = property.Value.ToString();
                    return string.IsNullOrEmpty(text) == false
                        ? text
                        : string.Empty;
                }
            }
        }
    }
}
