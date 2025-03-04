using System.Collections.Generic;
using System.Linq;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class FilterComposite : IFilter
    {
        public FilterComposite(Operator @operator, IEnumerable<IFilter> filters)
        {
            Operator = @operator;
            ChildFilters = filters.ToArray();
        }

        public FilterComposite(Operator @operator, params IFilter[] filters)
        {
            Operator = @operator;
            ChildFilters = filters;
        }

        public FilterComposite(IFilter filter)
        {
            Operator = Operator.And;
            ChildFilters = new List<IFilter> { filter };
        }

        public Operator Operator { get; }

        public IList<IFilter> ChildFilters { get; }

        public override string ToString()
        {
            return this.ToQueryString();
        }

        public string ToQueryString()
        {
            return $"({string.Join($" {this.Operator.ToString().ToLowerInvariant()} ", ChildFilters.Select(f => f.ToQueryString()))})";
        }
    }
}
