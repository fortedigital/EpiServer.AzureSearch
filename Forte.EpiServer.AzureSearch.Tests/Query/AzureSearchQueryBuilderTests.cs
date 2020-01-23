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
            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", "Value"))),
                    "(Field eq 'Value')")
                .SetName("Simple single value with string");

            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", 1))),
                    "(Field eq 1)")
                .SetName("Simple single value with int");

            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field", true))),
                    "(Field eq true)")
                .SetName("Simple single value with bool");

            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field2", 2))),
                    "(Field1 eq 1) and (Field2 eq 2)")
                .SetName("Two values combined with default operator and");
            
            yield return new TestCaseData(new AzureSearchQueryBuilder(Operator.Or)
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field2", 2))),
                    "(Field1 eq 1) or (Field2 eq 2)")
                .SetName("Two values combined with perator or");

            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(AzureSearchQueryFilter.Equals("Field1", 1)))
                        .Filter(new FilterComposite(Operator.Or, AzureSearchQueryFilter.Equals("Field2", 2), AzureSearchQueryFilter.Equals("Field3", 3))),
                    "(Field1 eq 1) and (Field2 eq 2 or Field3 eq 3)")
                .SetName("Grouped expressions");
            
            yield return new TestCaseData(new AzureSearchQueryBuilder()
                        .Filter(new FilterComposite(Operator.And, new List<IFilter>
                        {
                            {AzureSearchQueryFilter.Equals("Field1", 1)},
                            new FilterComposite(Operator.Or, AzureSearchQueryFilter.Equals("Field2", 2), AzureSearchQueryFilter.Equals("Field3", 3))
                        })),
                    "(Field1 eq 1 and (Field2 eq 2 or Field3 eq 3))")
                .SetName("Grouped expressions setup in single Filter call");
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
