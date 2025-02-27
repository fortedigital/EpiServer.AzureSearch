using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using Forte.EpiServer.AzureSearch.Plugin.Filters;
using NUnit.Framework;

namespace Forte.EpiServer.AzureSearch.Tests.Filters
{
    [TestFixture]
    public class ShortcutFilterTests
    {
        [Test]
        public void GivenNonPageDataContent_WhenCheckingIfShouldBeIndexed_ThenReturnTrue()
        {
            // Arrange
            var content = new BasicContent();
            var filter = new ShortcutFilter();

            // Act
            var shouldBeIndexed = filter.ShouldIndexContent(content);

            // Assert
            Assert.That(shouldBeIndexed, Is.True);
        }

        [Test]
        public void GivenPageDataContentWithDefaultShortcutType_WhenCheckingIfShouldBeIndexed_ThenReturnTrue()
        {
            // Arrange
            var content = new TestPageData
            {
                LinkType = PageShortcutType.Normal,
            };

            var filter = new ShortcutFilter();

            // Act
            var shouldBeIndexed = filter.ShouldIndexContent(content);

            // Assert
            Assert.That(shouldBeIndexed, Is.True);
        }

        [Test]
        public void GivenPageDataContentWithFetchDataShortcutType_WhenCheckingIfShouldBeIndexed_ThenReturnTrue()
        {
            // Arrange
            var content = new TestPageData
            {
                LinkType = PageShortcutType.FetchData,
            };

            var filter = new ShortcutFilter();

            // Act
            var shouldBeIndexed = filter.ShouldIndexContent(content);

            // Assert
            Assert.That(shouldBeIndexed, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ShortcutTypesProvider))]
        public void GivenPageDataContentWithAnyShortcutType_WhenCheckingIfShouldBeIndexed_ThenReturnFalse(PageShortcutType shortcutType)
        {
            // Arrange
            var content = new TestPageData
            {
                LinkType = shortcutType,
            };

            var filter = new ShortcutFilter();

            // Act
            var shouldBeIndexed = filter.ShouldIndexContent(content);

            // Assert
            Assert.That(shouldBeIndexed, Is.False);
        }

        private static IEnumerable<TestCaseData> ShortcutTypesProvider()
        {
            return Enum.GetValues<PageShortcutType>().Where(type => type is not PageShortcutType.Normal and not PageShortcutType.FetchData)
                .Select(type => new TestCaseData(type));
        }

        private class TestPageData : PageData
        {
            public override PageShortcutType LinkType { get; set; }
        }
    }
}
