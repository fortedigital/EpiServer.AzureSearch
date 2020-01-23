namespace Forte.EpiServer.AzureSearch.Query
{
    public interface ISearchPropertyValue
    {
        /// <summary>
        /// Method returns string which should be used in azure query string.
        /// <example>
        /// public class DateTimeOffsetPropertyValue : ISearchPropertyValue
        /// {
        ///     private readonly DateTimeOffset _dateTimeOffset;
        ///    
        ///     public DateTimeOffsetPropertyValue(DateTimeOffset dateTimeOffset)
        ///     {
        ///         _dateTimeOffset = dateTimeOffset;
        ///     }
        ///    
        ///     public string ToSearchQueryString()
        ///     {
        ///        return _dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ");
        ///     }
        /// }
        /// </example>
        /// </summary>
        /// <returns></returns>
        string ToSearchQueryString();
    }
}
