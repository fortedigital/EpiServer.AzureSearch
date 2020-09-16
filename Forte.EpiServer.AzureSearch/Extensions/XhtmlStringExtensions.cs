using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public static class XhtmlStringExtensions
    {
        public static string GetPlainTextContent(this XhtmlString xhtmlString)
        {
            var textBuilder = new StringBuilder();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var blockExtractors = ServiceLocator.Current.GetInstance<IEnumerable<IBlockContentExtractor>>();
            
            foreach (var fragment in xhtmlString.Fragments.GetFilteredFragments(PrincipalInfo
                .AnonymousPrincipal))
            {
                switch (fragment)
                {
                    case ContentFragment contentFragment when ContentReference.IsNullOrEmpty(contentFragment.ContentLink):
                        continue;
                    case ContentFragment contentFragment:
                    {
                        var content = contentLoader.Get<IContent>(contentFragment.ContentLink);
                        
                        var extractionResults = blockExtractors
                            .Where(e => e.CanExtract(content))
                            .Select(e => e.Extract(content))
                            .ToList();

                        var text = string.Join(" ", extractionResults.SelectMany(r => r.Values));
                        textBuilder.Append(text);
                        break;
                    }
                    case StaticFragment staticFragment:
                        var html = staticFragment.InternalFormat;
                        var htmlWithoutScripts = RemoveScripts(html);
                        var textFromHtml = StripHtml(htmlWithoutScripts);
                        var textFromHtmlWithTrailingSpace =
                            textFromHtml.EndsWith(" ") ? textFromHtml : textFromHtml + "";
                        textBuilder.Append(textFromHtmlWithTrailingSpace);
                        break;
                }
            }

            return textBuilder.ToString();
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
