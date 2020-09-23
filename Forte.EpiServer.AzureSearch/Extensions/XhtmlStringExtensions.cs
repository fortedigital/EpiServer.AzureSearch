using System.Collections.Generic;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor.Block;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public class XhtmlStringExtractor
    {
        private readonly BlockContentExtractor _blockContentExtractor;

        public XhtmlStringExtractor(BlockContentExtractor blockContentExtractor)
        {
            _blockContentExtractor = blockContentExtractor;
        }
        public string GetPlainTextContent( XhtmlString xhtmlString)
        {
            var texts = new List<string>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

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
                        var content = contentLoader.Get<IContent>(contentFragment.ContentLink);
                        
                        var text = _blockContentExtractor.ExtractTextFromBlock(content);
                        texts.Add(text);
                        break;
                    }
                    case StaticFragment staticFragment:
                        var html = staticFragment.InternalFormat;
                        var htmlWithoutScripts = RemoveScripts(html);

                        texts.Add(htmlWithoutScripts);
                        break;
                }
            }

            var joinedText = string.Join(BlockContentExtractorController.BlockExtractedTextFragmentsSeparator,
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
