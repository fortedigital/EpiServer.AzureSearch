using System;
using Microsoft.Azure.Search;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class ContentDocument : SearchDocument
    {
        [IsSearchable]
        public string[] ContentBody { get; set; }
		
        public int ContentId { get; set; }
        public string ContentImageUrl { get; set; }
        public int ContentImageReferenceId { get; set; }
		
        [IsFilterable]
        public string ContentLanguage { get; set; }
		
        [IsSearchable]
        public string ContentName { get; set; }
        
        public int[] ContentPath { get; set; }
        
        [IsFilterable]
        public int ContentTypeId { get; set; }
		
        [IsFacetable]
        [IsFilterable]
        public string ContentTypeName { get; set; }
                
        public string ContentUrl { get; set; }
        
        [IsFilterable]
        public DateTimeOffset? StopPublishUtc { get; set; }
    }
}
