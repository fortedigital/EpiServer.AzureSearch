namespace Forte.EpiServer.AzureSearch.Configuration
{
    public class NullIndexSpecificationProvider : IIndexSpecificationProvider
    {
        public IIndexSpecification GetIndexSpecification()
        {
            return null;
        }
    }
}