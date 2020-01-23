using AngleSharp;
using AngleSharp.Dom;
using EPiServer.Core;
using EPiServer.Core.Html;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class XhtmlStringExtensions
    {
        public static string GetPlainTextContent(this XhtmlString xhtmlString)
        {
            var htmlWithoutScripts = RemoveScripts(xhtmlString.ToHtmlString());
            //TODO: try to inline content of embeded blocks
            return StripHtml(htmlWithoutScripts);
        }

        private static string StripHtml(string html)
        {
            return TextIndexer.StripHtml(html, int.MaxValue, int.MaxValue, "...");
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
