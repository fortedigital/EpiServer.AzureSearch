namespace Forte.EpiServer.AzureSearch.Configuration
{
    public class PrefixedIndexNamingConvention : IIndexNamingConvention
    {
        private readonly string _prefix;

        public PrefixedIndexNamingConvention(string prefix)
        {
            _prefix = prefix;
        }

        public PrefixedIndexNamingConvention()
            : this("ForteAzureSearch")
        {
        }

        public string GetIndexName(string indexName)
        {
            return (string.IsNullOrEmpty(_prefix)
                ? indexName
                : $"{_prefix}{indexName}").ToLower();
        }
    }
}
