namespace Forte.EpiServer.AzureSearch
{
    public interface IIndexNamingConvention
    {
        string GetIndexName(string indexName);
    }
}