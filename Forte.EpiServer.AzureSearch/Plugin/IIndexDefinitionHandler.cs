using System.Threading.Tasks;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public interface IIndexDefinitionHandler
    {
        Task<UpdateOrRecreateResult> UpdateOrRecreateIndex();
    }
}
