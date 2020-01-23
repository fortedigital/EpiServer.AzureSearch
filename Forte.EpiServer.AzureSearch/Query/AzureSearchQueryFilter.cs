namespace Forte.EpiServer.AzureSearch.Query
{
    public class AzureSearchQueryFilter : IFilter
    {
        public AzureSearchQueryFilter(string propertyName, ComparisonExpression comparison, ISearchPropertyValue value)
        {
            PropertyName = propertyName;
            Comparison = comparison;
            Value = value;
        }
        
        public AzureSearchQueryFilter(string propertyName, ComparisonExpression comparison, object value)
        {
            PropertyName = propertyName;
            Comparison = comparison;
            Value = value;
        }

        public static AzureSearchQueryFilter Equals(string propertyName, ISearchPropertyValue value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Eq, value);
        }
        
        public static AzureSearchQueryFilter Equals(string propertyName, object value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Eq, value);
        }
        
        public static AzureSearchQueryFilter NotEquals(string propertyName, object value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Ne, value);
        }
        
        public static AzureSearchQueryFilter NotEquals(string propertyName, ISearchPropertyValue value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Ne, value);
        }

        public static AzureSearchQueryFilter LessThan(string propertyName, object value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Lt, value);
        }
        
        public static AzureSearchQueryFilter LessThan(string propertyName, ISearchPropertyValue value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Lt, value);
        }

        public static AzureSearchQueryFilter GreaterThan(string propertyName, object value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Gt, value);
        }
        
        public static AzureSearchQueryFilter GreaterThan(string propertyName, ISearchPropertyValue value)
        {
            return new AzureSearchQueryFilter(propertyName, ComparisonExpression.Gt, value);
        }

        public string PropertyName { get; }
        public ComparisonExpression Comparison { get; }
        public GroupingExpression? GroupingExpression { get; set; }
        public object Value { get; }

        private static string GetValuePart(object value)
        {
            switch (value)
            {
                case null:
                    return "null";
                case ISearchPropertyValue searchParameterValue:
                    return searchParameterValue.ToSearchQueryString();
                case int intValue:
                    return intValue.ToString();
                case bool boolValue:
                    return boolValue.ToString().ToLowerInvariant();
                default:
                    return $"'{GetSafeString(value.ToString())}'";
            }
        }

        private static string GetSafeString(string input)
        {
            return input.Replace("'", "''");
        }

        public string ToQueryString()
        {
            var valuePart = GetValuePart(Value);
            var comparisonPart = Comparison.ToString().ToLowerInvariant();
            
            if (GroupingExpression.HasValue)
            {
                var groupingExpressionPart = GroupingExpression.Value.ToString().ToLowerInvariant();
                
                return $"{PropertyName}/{groupingExpressionPart}(p: p {comparisonPart} {valuePart})";
            }

            return $"{PropertyName} {comparisonPart} {valuePart}";
        }
    }
}
