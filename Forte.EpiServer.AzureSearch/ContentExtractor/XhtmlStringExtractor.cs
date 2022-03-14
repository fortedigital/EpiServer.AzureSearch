using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using AngleSharp;
using AngleSharp.Dom;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Core.Html.StringParsing;

namespace Forte.EpiServer.AzureSearch.ContentExtractor
{
    public class XhtmlStringExtractor
    {
        private readonly IContentLoader _contentLoader;

        public XhtmlStringExtractor(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public string GetPlainTextContent(XhtmlString xhtmlString, IContentExtractorController extractor)
        {
            if (xhtmlString == null)
            {
                return string.Empty;
            }

            var texts = new List<string>();
            var staticFragments = new List<string>();

            var xhtmlFragments = xhtmlString
                .Fragments
                .GetFilteredFragments(GetAnonymousPrincipal());

            foreach (var fragment in xhtmlFragments)
            {
                switch (fragment)
                {
                    case ContentFragment contentFragment when ContentReference.IsNullOrEmpty(contentFragment.ContentLink):
                        continue;
                    case ContentFragment contentFragment:
                    {
                        if (staticFragments.Any())
                        {
                            texts.Add(RemoveScripts(staticFragments));
                            staticFragments.Clear();
                        }

                        var content = _contentLoader.Get<IContent>(contentFragment.ContentLink);

                        texts.Add(extractor.ExtractBlock(content));
                        break;
                    }
                    case StaticFragment staticFragment:
                        var html = staticFragment.InternalFormat;
                        staticFragments.Add(html);
                        break;
                }
            }

            if (staticFragments.Any())
            {
                texts.Add(RemoveScripts(staticFragments));
            }

            var joinedText = string.Join(ContentExtractorController.BlockExtractedTextFragmentsSeparator,
                texts.Select(t => t.Trim()));
            return StripHtml(joinedText);
        }

        private static IPrincipal GetAnonymousPrincipal() => new GenericPrincipal(new GenericIdentity(string.Empty), null);

        private static string StripHtml(string html)
        {
            const string moreTextMarker = "...";
            return TextIndexer.StripHtml(html, int.MaxValue, int.MaxValue, moreTextMarker).Trim();
        }

        private static string RemoveScripts(IEnumerable<string> strings)
        {
            return RemoveScripts(string.Join("", strings));
        }

        private static string RemoveScripts(string htmlString)
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument("<html><body></body></html>");

            var htmlFragment = parser.ParseFragment(htmlString, document.Body);
            foreach (var scriptElement in htmlFragment.QuerySelectorAll("script"))
            {
                scriptElement.Remove();
            }

            return htmlFragment.ToHtml();
        }
    }
}
