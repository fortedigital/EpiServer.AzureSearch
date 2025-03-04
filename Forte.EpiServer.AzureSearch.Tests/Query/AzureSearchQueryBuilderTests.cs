using System;
using System.Collections.Generic;
using Forte.EpiServer.AzureSearch.Query;
using NUnit.Framework;

namespace Forte.EpiServer.AzureSearch.Tests.Query
{
    [TestFixture]
    public class AzureSearchQueryBuilderTests
    {
        private static IEnumerable<TestCaseData> GetFiltersWithExpectedStrings()
        {
            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", "Value"))),
                    "(Field eq 'Value')")
                .SetName("Simple single value with string");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", 1))),
                    "(Field eq 1)")
                .SetName("Simple single value with int");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", true))),
                    "(Field eq true)")
                .SetName("Simple single value with bool");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field2", 2))),
                    "(Field1 eq 1) and (Field2 eq 2)")
                .SetName("Two values combined with default operator and");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder(Operator.Or)
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field2", 2))),
                    "(Field1 eq 1) or (Field2 eq 2)")
                .SetName("Two values combined with perator or");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(Operator.Or, AzureSearchQueryFilter.Equals("Field2", 2), AzureSearchQueryFilter.Equals("Field3", 3))),
                    "(Field1 eq 1) and (Field2 eq 2 or Field3 eq 3)")
                .SetName("Grouped expressions");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(
                            new FilterComposite(
                                Operator.And,
                                new List<IFilter>
                                {
                                    { AzureSearchQueryFilter.Equals("Field1", 1) },
                                    new FilterComposite(Operator.Or, AzureSearchQueryFilter.Equals("Field2", 2), AzureSearchQueryFilter.Equals("Field3", 3))
                                })),
                    "(Field1 eq 1 and (Field2 eq 2 or Field3 eq 3))")
                .SetName("Grouped expressions setup in single Filter call");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder().Filter(new NotFilter(AzureSearchQueryFilter.Equals("Field1", 1))),
                    "(not Field1 eq 1)")
                .SetName("Negated Filter");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder().Filter(
                        new AzureSearchFilterSearchIn(
                            "Field1",
                            new[]
                            {
                                "1",
                                "120",
                                "22"
                            })),
                    "(search.in(Field1, '1 120 22', ' '))")
                .SetName("Filter with search.in function");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder().Filter(new AzureSearchFilterSearchIn("Field1", Array.Empty<string>())),
                    "(search.in(Field1, '', ' '))")
                .SetName("Filter with search.in function but with empty enumerable");

            yield return new TestCaseData(
                    new AzureSearchQueryBuilder()
                        .Filter(AzureSearchQueryFilter.GreaterThan("Field1", 500))
                        .Filter(
                            new NotFilter(
                                new AzureSearchFilterSearchIn(
                                    "Field2",
                                    new[]
                                    {
                                        "1",
                                        "120",
                                        "22"
                                    },
                                    ", "))),
                    "(Field1 gt 500) and (not search.in(Field2, '1, 120, 22', ', '))")
                .SetName("Filter with negated search.in function and simple greater than query");
        }

        [Test]
        [TestCaseSource(nameof(GetFiltersWithExpectedStrings))]
        public void GivenQueryBuilder_ThenFilterValueIsProper(AzureSearchQueryBuilder builder, string expectedFilterValue)
        {
            var query = builder.Build();

            Assert.That(query.Filter, Is.EqualTo(expectedFilterValue));
        }
    }
}
