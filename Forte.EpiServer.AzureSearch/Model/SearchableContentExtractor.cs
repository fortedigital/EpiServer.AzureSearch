using System.Collections.Generic;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Extensions;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class SearchableContentExtractor : IContentExtractor
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
                else if (property.Value != null)
                {
                    stringValues.Add(property.Value.ToString());
                }
            }

            return new ContentExtractionResult(stringValues, null);
        }
    }
}
