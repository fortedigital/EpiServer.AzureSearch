using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public interface IContentDocumentBuilder<out T>
        where T : ContentDocument
    {
        T Build(IContent content);
        T Build(PageData pageData);
    }
}
