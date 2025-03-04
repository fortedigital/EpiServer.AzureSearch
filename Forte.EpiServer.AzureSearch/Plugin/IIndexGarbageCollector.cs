using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public interface IIndexGarbageCollector
    {
        Task RemoveOutdatedContent(DateTimeOffset olderThan, IList<ContentReference> contentReferencesToPreserve);
    }
}
