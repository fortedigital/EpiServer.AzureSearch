using System;
using System.Collections.Generic;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexStatistics
    {
        public IList<ContentReference> FailedContentReferences { get; } = new List<ContentReference>();
        public IList<Exception> Exceptions { get; } = new List<Exception>();
    }
}
