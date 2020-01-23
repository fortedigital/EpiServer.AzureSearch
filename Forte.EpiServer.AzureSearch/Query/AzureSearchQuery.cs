using System.Collections.Generic;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class AzureSearchQuery
    {
        public string SearchTerm { get; set; }
        public int Top { get; set; } = 1000;
        public IList<string> HighlightFields { get; } = new List<string>();
        public IList<string> Facets { get; } = new List<string>();
        public string Filter { get; set; }
        public string HighlightPreTag { get; set; }
        public string HighlightPostTag { get; set; }
        public int Skip { get; set; }
        public string ScoringProfile { get; set; }
    }
}
