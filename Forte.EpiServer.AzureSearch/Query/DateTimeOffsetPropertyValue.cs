using System;

namespace Forte.EpiServer.AzureSearch.Query
{
    public class DateTimeOffsetPropertyValue : ISearchPropertyValue
    {
        private readonly DateTimeOffset _dateTimeOffset;

        public DateTimeOffsetPropertyValue(DateTimeOffset dateTimeOffset)
        {
            _dateTimeOffset = dateTimeOffset;
        }

        public string ToSearchQueryString()
        {
            return _dateTimeOffset.ToString("O");
        }
    }
}
