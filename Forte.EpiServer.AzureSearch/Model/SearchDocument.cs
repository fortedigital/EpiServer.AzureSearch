using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;

namespace Forte.EpiServer.AzureSearch.Model
{
    public class SearchDocument
    {
        [Key]
        public string Id { get; set; }

        [IsFilterable]
        public DateTimeOffset IndexedAt { get; set; }
    }
}
