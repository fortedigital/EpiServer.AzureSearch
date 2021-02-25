using System.Linq;
using EPiServer.Security;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Query.Extensions
{
    public static class FilterByAccessExtensions
    {
        /// <summary>
        /// Method filters search items by allowed roles and users
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <returns></returns>
        public static AzureSearchQueryBuilder FilterOnReadAccess(this AzureSearchQueryBuilder queryBuilder)
        {
            var roles = PrincipalInfo.Current?.RoleList ?? Enumerable.Empty<string>();
            var user = PrincipalInfo.Current?.Name ?? string.Empty;
            
            var accessUserFilter = new AzureSearchQueryFilter(nameof(ContentDocument.AccessUsers), ComparisonExpression.Eq, user)
            {
                GroupingExpression = GroupingExpression.Any
            };
            
            var accessRoleQueries = roles
                .Select(roleName => new AzureSearchQueryFilter(nameof(ContentDocument.AccessRoles),
                    ComparisonExpression.Eq, roleName)
                {
                    GroupingExpression = GroupingExpression.Any
                })
                .Append(accessUserFilter);

            return queryBuilder.Filter(new FilterComposite(Operator.Or, accessRoleQueries));
        }
    }
}
