using System.Collections.Generic;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;

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
                .GetFilteredFragments(PrincipalInfo.AnonymousPrincipal);
            
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
                            texts.Add(RemoveScripts(string.Join("", staticFragments)));
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
                texts.Add(RemoveScripts(string.Join("", staticFragments)));
            }

            var joinedText = string.Join(ContentExtractorController.BlockExtractedTextFragmentsSeparator,
                texts.Select(t => t.Trim()));
            return StripHtml(joinedText);
        }

        private static string StripHtml(string html)
        {
            const string moreTextMarker = "...";
            return TextIndexer.StripHtml(html, int.MaxValue, int.MaxValue, moreTextMarker);
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
