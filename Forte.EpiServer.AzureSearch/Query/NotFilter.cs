namespace Forte.EpiServer.AzureSearch.Query
{
    public class NotFilter : IFilter
    {
        private readonly IFilter _filter;

        public NotFilter(IFilter filter)
        {
            _filter = filter;
        }

        public string ToQueryString() => $"not {_filter.ToQueryString()}";
    }
}
