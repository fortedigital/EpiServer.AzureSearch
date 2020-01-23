namespace Forte.EpiServer.AzureSearch.Configuration
{
    public class AzureSearchServiceConfiguration
    {
        public string ServiceName { get; }
        public string ApiKey { get; }

        public AzureSearchServiceConfiguration(string serviceName, string apiKey)
        {
            ServiceName = serviceName;
            ApiKey = apiKey;
        }
    }
}
