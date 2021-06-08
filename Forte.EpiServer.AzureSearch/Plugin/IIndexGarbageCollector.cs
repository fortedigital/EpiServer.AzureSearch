using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public interface IIndexGarbageCollector
    {
        Task RemoveOutdatedContent(DateTimeOffset olderThan, IList<int> contentIdsToPreserve);
    }
}