using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class IndexableContentExtractor : IIndexableContentExtractor
    {
        private readonly IContentLoader _contentLoader;
        private readonly BlockContentExtractor _blockContentExtractor;
        private readonly XhtmlStringExtractor _xhtmlStringExtractor;

        public IndexableContentExtractor(IContentLoader contentLoader, BlockContentExtractor blockContentExtractor, XhtmlStringExtractor xhtmlStringExtractor)
        {
            _contentLoader = contentLoader;
            _blockContentExtractor = blockContentExtractor;
            _xhtmlStringExtractor = xhtmlStringExtractor;
        }
        public bool CanExtract(IContentData content)
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
                        stringValues.Add(_xhtmlStringExtractor.GetPlainTextContent(xhtmlString));
                        break;
                    case BlockData localBlock:
                        stringValues.Add(_blockContentExtractor.ExtractTextFromBlock(localBlock));
                        break;
                    //TODO: check if this case is used ORM blockData
                    case ContentReference contentReference:
                    {
                        var propertyContent = _contentLoader.Get<IContent>(contentReference);

                        if (propertyContent is BlockData == false)
                        {
                            continue;
                        }
                        stringValues.Add(_blockContentExtractor.ExtractTextFromBlock(propertyContent));
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
