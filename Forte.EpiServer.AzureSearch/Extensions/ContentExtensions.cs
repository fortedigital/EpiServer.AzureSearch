using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class ContentExtensions
    {
        public static string GetDocumentUniqueId(this IContent content)
        {
            var language = string.Empty;

            if (content is ILocalizable localizable)
            {
                language = localizable.Language.Name;
            }

            return content.ContentLink.GetDocumentUniqueId(language);
        }

        private static string GetDocumentUniqueId(this ContentReference contentReference, string language)
        {
            var builder = new StringBuilder();

            builder.Append(contentReference.ID);

            if (string.IsNullOrEmpty(language) == false)
            {
                builder.Append($"_{language}");
            }

            var contentProviderName = contentReference.ProviderName;

            if (string.IsNullOrEmpty(contentProviderName) == false)
            {
                builder.Append($"_{contentProviderName}");
            }

            return builder.ToString();
        }

        public static IEnumerable<PropertyData> GetIndexableProperties(this IContentData content, Func<PropertyData, bool> propertyPredicate = null)
        {
            var predicate = propertyPredicate ?? (_ => true);
            var propertyDataCollection = content.Property.Where(predicate);

            var contentType = content.GetType();

            foreach (var propertyData in propertyDataCollection)
            {
                var property = contentType.GetProperty(propertyData.Name);

                if (property == null)
                {
                    continue;
                }

                var indexableAttribute = property.GetCustomAttribute<IndexableAttribute>();

                if (indexableAttribute != null && indexableAttribute.IsIndexable)
                {
                    yield return propertyData;
                }
            }
        }
    }
}
