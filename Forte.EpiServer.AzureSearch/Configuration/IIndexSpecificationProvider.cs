namespace Forte.EpiServer.AzureSearch.Configuration
{
    public interface IIndexSpecificationProvider
    {
        IIndexSpecification GetIndexSpecification();
    }
}
