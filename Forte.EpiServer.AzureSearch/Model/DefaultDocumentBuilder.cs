using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Forte.EpiServer.AzureSearch.ContentExtractor;
using Forte.EpiServer.AzureSearch.Extensions;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class DefaultDocumentBuilder : DefaultDocumentBuilder<ContentDocument>
    {
        public DefaultDocumentBuilder(
            IUrlResolver urlResolver,
            IContentLoader contentLoader,
            IContentExtractorController extractor,
            IContentTypeRepository contentTypeRepository)
            : base(urlResolver, contentLoader, extractor, contentTypeRepository)
        {
        }
    }

    public abstract class DefaultDocumentBuilder<T> : IContentDocumentBuilder<T>
        where T : ContentDocument, new()
    {
        protected readonly IUrlResolver UrlResolver;
        protected readonly IContentLoader ContentLoader;
        protected readonly IContentExtractorController Extractor;
        protected readonly IContentTypeRepository ContentTypeRepository;

        protected DefaultDocumentBuilder(
            IUrlResolver urlResolver,
            IContentLoader contentLoader,
            IContentExtractorController extractor,
            IContentTypeRepository contentTypeRepository)
        {
            UrlResolver = urlResolver;
            ContentLoader = contentLoader;
            Extractor = extractor;
            ContentTypeRepository = contentTypeRepository;
        }

        public virtual T Build(IContent content)
        {
            var document = new T
            {
                Id = content.GetDocumentUniqueId(),
                ContentId = content.ContentLink.ID,
                ContentComplexReference = content.ContentLink.ToString(),
                ContentName = content.Name,
            };

            var languageName = string.Empty;

            if (content is ILocalizable localizable)
            {
                languageName = localizable.Language.Name;
                document.ContentLanguage = languageName;
            }

            document.ContentUrl = EnsureRelativeUrl(UrlResolver.GetUrl(content.ContentLink, languageName));

            if (content is ISearchableWithImage searchableWithImage && searchableWithImage.SearchResultsImage != null)
            {
                document.ContentImageUrl = GetImageUrl(searchableWithImage.SearchResultsImage, languageName);
                document.ContentImageReferenceId = searchableWithImage.SearchResultsImage.ID;
            }

            if (content is PageData pageData)
            {
                document.ContentTypeName = pageData.PageTypeName;

                document.StopPublishUtc = pageData.StopPublish.HasValue
                    ? new DateTimeOffset(pageData.StopPublish.Value)
                    : null;
            }
            else
            {
                document.ContentTypeName = ContentTypeRepository.Load(content.ContentTypeID).Name;
            }

            if (content is IChangeTrackable changeTrackableContent)
            {
                document.CreatedAt = changeTrackableContent.Created;
            }

            document.AccessRoles = GetReadAccessEntriesNames(content, SecurityEntityType.Role);
            document.AccessUsers = GetReadAccessEntriesNames(content, SecurityEntityType.User);

            document.ContentTypeId = content.ContentTypeID;

            var contentAncestors = ContentLoader.GetAncestors(content.ContentLink);

            document.ContentPath = contentAncestors.Reverse().Skip(1).Select(c => c.ContentLink.ID).ToArray();
            document.ContentBody = Extractor.ExtractPage(content).ToArray();

            return document;
        }

        private static string[] GetReadAccessEntriesNames(IContent content, SecurityEntityType securityEntityType)
        {
            return content.ToRawACEArray()
                .Where(ace => ace.Access.HasFlag(AccessLevel.Read) && ace.EntityType == securityEntityType)
                .Select(ace => ace.Name)
                .ToArray();
        }

        public virtual T Build(PageData pageData)
        {
            return Build(pageData as IContent);
        }

        protected virtual string GetImageUrl(ContentReference contentReference, string languageName = null)
        {
            return EnsureRelativeUrl(UrlResolver.GetUrl(contentReference, languageName));
        }

        private static string EnsureRelativeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            return uri.IsAbsoluteUri
                ? uri.AbsolutePath
                : url;
        }
    }
}
