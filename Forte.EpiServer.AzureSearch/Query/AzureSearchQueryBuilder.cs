using System.Collections.Generic;
using System.Linq;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class AzureSearchQueryBuilder
    {
        private readonly Operator _filterOperator;
        
        public AzureSearchQueryBuilder() : this(Operator.And)
        {
        }

        public AzureSearchQueryBuilder(Operator filterOperator)
        {
            _filterOperator = filterOperator;
        }

        private readonly AzureSearchQuery _query = new AzureSearchQuery();
        private readonly IList<FilterComposite> _filters = new List<FilterComposite>();

        public AzureSearchQueryBuilder SearchTerm(string term)
        {
            _query.SearchTerm = term;
            
            return this;
        }

        public AzureSearchQueryBuilder Top(int count)
        {
            _query.Top = count;
            
            return this;
        }

        public AzureSearchQueryBuilder HighlightFields(params string[] fields)
        {
            foreach (var field in fields ?? new string[0])
            {
                _query.HighlightFields.Add(field);
            }

            HighlightField.Bold.Apply(_query);
            
            return this;
        }

        public AzureSearchQueryBuilder Facets(params string[] facets)
        {
            foreach (var facet in facets ?? new string[0])
            {
                _query.Facets.Add(facet);
            }

            return this;
        }

        public AzureSearchQueryBuilder Filter(FilterComposite filterComposite)
        {
            _filters.Add(filterComposite);
        
            return this;
        }
        
        public AzureSearchQueryBuilder Filter(IFilter filter)
        {
            this._filters.Add(new FilterComposite(_filterOperator, new List<IFilter> {filter}));
        
            return this;
        }

        public AzureSearchQueryBuilder FilterByContentTypeId(int contentTypeId)
        {
            var filter = AzureSearchQueryFilter.Equals(nameof(ContentDocument.ContentTypeId), contentTypeId);
            
            return Filter(filter);
        }
        
        public AzureSearchQueryBuilder WithScoringProfile(string scoringProfile)
        {
            _query.ScoringProfile = scoringProfile;

            return this;
        }
        
        public AzureSearchQuery Build()
        {
            _query.Filter = string.Join($" {_filterOperator.ToString().ToLowerInvariant()} ", _filters.Select(d => d.ToQueryString()));            
            
            return _query;
        }

        public AzureSearchQueryBuilder Skip(int count)
        {
            _query.Skip = count;
            
            return this;
        }

        public AzureSearchQueryBuilder OrderBy(IList<string> orderBy)
        {
            _query.OrderBy = orderBy;

            return this;
        }
        
        private sealed class HighlightField
        {
            public static HighlightField Bold => new HighlightField("<b>", "</b>");
            private readonly string _preTag;
            private readonly string _postTag;
            private HighlightField(string preTag, string postTag)
            {
                _preTag = preTag;
                _postTag = postTag;
            }
            public void Apply(AzureSearchQuery query)
            {
                query.HighlightPreTag = _preTag;
                query.HighlightPostTag = _postTag;
            }
        }
    }
}
