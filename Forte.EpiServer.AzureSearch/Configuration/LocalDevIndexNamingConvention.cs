using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Forte.EpiServer.AzureSearch.Configuration
{
    public class LocalDevIndexNamingConvention : IIndexNamingConvention
    {
        private readonly PrefixedIndexNamingConvention _prefixedIndexNamingConvention;

        public LocalDevIndexNamingConvention(PrefixedIndexNamingConvention prefixedIndexNamingConvention)
        {
            _prefixedIndexNamingConvention = prefixedIndexNamingConvention;
        }

        public string GetIndexName(string indexName)
        {
            var prefixedIndexName = _prefixedIndexNamingConvention.GetIndexName(indexName);

            var environmentName = ConfigurationManager.AppSettings["episerver:EnvironmentName"];

            if (environmentName.Equals("LocalDev"))
            {
                var machineNameNormalized = Regex.Replace(Environment.MachineName, @"[^\w]", "").ToLower();

                return $"{prefixedIndexName}{machineNameNormalized}";
            }

            return prefixedIndexName;
        }
    }
}
