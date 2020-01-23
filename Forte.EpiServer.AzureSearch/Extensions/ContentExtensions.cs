using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Filters;
using EPiServer.Security;

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
        
        public static string GetDocumentUniqueId(this ContentReference contentReference, string language)
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
        
        public static IEnumerable<PropertyData> GetSearchableProperties(this IContent content, Func<PropertyData, bool> propertyPredicate = null)
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
                
                var searchableAttribute = property.GetCustomAttribute<SearchableAttribute>();
                if (searchableAttribute != null && searchableAttribute.IsSearchable)
                {
                    yield return propertyData;
                }
            }
        }

        public static bool ShouldIndex(this IContent content)
        {
            var filterPublished = new FilterPublished();
            var filterTemplate = new FilterTemplate();
            var anonymousHasAccess = FilterAccess.QueryDistinctAccessEdit(content, AccessLevel.Read, PrincipalInfo.AnonymousPrincipal);
            var hasTemplate = !filterTemplate.ShouldFilter(content);
            var isPublished = !filterPublished.ShouldFilter(content);
            
            return anonymousHasAccess && hasTemplate && isPublished;
        }
    }
}
