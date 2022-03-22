using System;
using System.Collections.Generic;
using System.Threading;
using EPiServer.Core;

namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class IndexContentRequest
    {
        public readonly int ExceptionThreshold = 20;
        public Action<string> OnStatusChanged { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
        public HashSet<ContentReference> IgnoreContent { get; set; }
        public HashSet<ContentReference> VisitedContent { get; set; }
        public IndexStatistics Statistics { get; set; }
    }
}
