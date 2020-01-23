using System.Collections.Generic;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Model
{
    public interface IContentExtractorController
    {
        IEnumerable<string> Extract(IContent content);
    }
}