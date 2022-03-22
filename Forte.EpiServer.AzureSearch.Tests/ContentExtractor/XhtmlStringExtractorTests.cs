using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Web.Routing;
using Forte.EpiServer.AzureSearch.ContentExtractor;
using Moq;
using NUnit.Framework;

namespace Forte.EpiServer.AzureSearch.Tests.ContentExtractor
{
    [TestFixture]
    public class XhtmlStringExtractorTests
    {
        private XhtmlStringExtractor _xhtmlStringExtractor;
        private IContentExtractorController _contentExtractorController;
        
        [SetUp]
        public void SetUp()
        {
            var contentLoader = new Mock<IContentLoader>();
            _xhtmlStringExtractor = new XhtmlStringExtractor(contentLoader.Object);
            _contentExtractorController = new Mock<IContentExtractorController>().Object;
        }

        [Test]
        [TestCaseSource(nameof(PlainTextContentSource))]
        public void GetPlainTextContentShouldReturnValidText(XhtmlString testString, string expectedString)
        {
            var strippedText = _xhtmlStringExtractor.GetPlainTextContent(testString, _contentExtractorController);
            
            Assert.That(strippedText, Is.EqualTo(expectedString));
        }
        
        private static IEnumerable<TestCaseData> PlainTextContentSource()
        {
            var urlResolver =  new Mock<IUrlResolver>().Object;
            var imgWithTextAfter = new XhtmlString();
            imgWithTextAfter.Fragments.Add(new StaticFragment("<img src=\""));
            imgWithTextAfter.Fragments.Add(new UrlFragment("~/link/1b108a4b8f9a4b47802f0112f5b07d11.aspx", urlResolver));
            imgWithTextAfter.Fragments.Add(new StaticFragment("\" alt=\"alt\"/><p>text</p>"));
            
            yield return new TestCaseData(imgWithTextAfter, "text").SetName("Img with plain text after");
            
            var imgWithTextBeforeAndAfter = new XhtmlString();
            imgWithTextBeforeAndAfter.Fragments.Add(new StaticFragment("<p>1</p>"));
            imgWithTextBeforeAndAfter.Fragments.Add(new StaticFragment("<img src=\""));
            imgWithTextBeforeAndAfter.Fragments.Add(new UrlFragment("~/link/1b108a4b8f9a4b47802f0112f5b07d11.aspx", urlResolver));
            imgWithTextBeforeAndAfter.Fragments.Add(new StaticFragment("\" alt=\"alt\"/><p>2</p>"));
            
            yield return new TestCaseData(imgWithTextBeforeAndAfter, "1 2").SetName("Img with plain text in before and after");
        }
    }
}
