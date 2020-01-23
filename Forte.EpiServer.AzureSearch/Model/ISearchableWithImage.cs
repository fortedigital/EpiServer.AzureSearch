using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public interface ISearchableWithImage
    {
        ContentReference SearchResultsImage { get; }
    }
}
