using System;
using System.Collections.Generic;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class AzureSearchQuery : ICloneable
    {
        public string SearchTerm { get; set; }
        private int _top = 1000;
        public int Top
        {
            get => _top;
            set 
            {
                if (value > 1000)
                {
                    throw new ArgumentException($"Max value of Top is 1000. Consider using {nameof(AzureSearchServiceExtension.SearchBatch)} extension method");
                }

                _top = value;
            }
        }
        public IList<string> HighlightFields { get; } = new List<string>();
        public IList<string> Facets { get; } = new List<string>();
        public string Filter { get; set; }
        public string HighlightPreTag { get; set; }
        public string HighlightPostTag { get; set; }
        public int Skip { get; set; }
        public string ScoringProfile { get; set; }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
