namespace Forte.EpiServer.AzureSearch.Configuration
{
    public class AzureSearchServiceOptions
    {
        public string ServiceName { get; }
        public string ApiKey { get; }

        public AzureSearchServiceOptions(string serviceName, string apiKey)
        {
            ServiceName = serviceName;
            ApiKey = apiKey;
        }
    }
}
