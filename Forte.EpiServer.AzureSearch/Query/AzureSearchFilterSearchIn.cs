using System.Collections.Generic;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class AzureSearchFilterSearchIn : IFilter
    {
        public AzureSearchFilterSearchIn(string propertyName, IEnumerable<string> possibleValues, string separator = " ")
        {
            PropertyName = propertyName;
            PossibleValues = possibleValues;
            Separator = separator;
        }

        public string PropertyName { get; }

        public IEnumerable<string> PossibleValues { get; }

        public string Separator { get; }

        public string ToQueryString()
        {
            return $"search.in({PropertyName}, '{string.Join(Separator, PossibleValues)}', '{Separator}')";
        }
    }
}
