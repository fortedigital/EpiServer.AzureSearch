using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class XhtmlStringExtensions
    {
        public static string GetPlainTextContent(this XhtmlString xhtmlString)
        {
            var textBuilder = new StringBuilder();
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
                        
                        var text = content.ExtractTextFromBlock();
                        textBuilder.Append(text);
                        break;
                    }
                    case StaticFragment staticFragment:
                        var html = staticFragment.InternalFormat;
                        var htmlWithoutScripts = RemoveScripts(html);
                        
                        const string whitespace = " ";
                        var htmlWithTrailingSpace =
                            htmlWithoutScripts.EndsWith(whitespace) ? htmlWithoutScripts : htmlWithoutScripts + whitespace;
                        
                        textBuilder.Append(htmlWithTrailingSpace);
                        break;
                }
            }
            
            return StripHtml(textBuilder.ToString());
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
