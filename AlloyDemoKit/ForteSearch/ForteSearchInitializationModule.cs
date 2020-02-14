using System.Configuration;
using EPiServer.Framework;
using Forte.EpiServer.AzureSearch.Configuration;

namespace AlloyDemoKit.ForteSearch
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ForteSearchInitializationModule : DefaultAzureSearchServiceInitializationModule
    {
        protected override AzureSearchServiceConfiguration GetSearchServiceConfiguration()
        {
            var serviceName = ConfigurationManager.AppSettings["AzureSearchService:Name"];
            var apiKey = ConfigurationManager.AppSettings["AzureSearchService:ApiKey"];
            
            return new AzureSearchServiceConfiguration(serviceName,apiKey);
        }
    }
}
