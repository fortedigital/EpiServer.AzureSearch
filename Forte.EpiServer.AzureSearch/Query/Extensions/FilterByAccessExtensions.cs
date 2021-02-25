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
        public static AzureSearchQueryBuilder FilterOnRoleAccess(this AzureSearchQueryBuilder queryBuilder)
        {
            var currentPrincipalRoles = PrincipalInfo.Current?.RoleList ?? Enumerable.Empty<string>();

            var rolesAccessFilters = currentPrincipalRoles
                .Select(roleName => new AzureSearchQueryFilter(nameof(ContentDocument.AccessRoles),
                    ComparisonExpression.Eq, roleName)
                {
                    GroupingExpression = GroupingExpression.Any
                });

            return queryBuilder.Filter(new FilterComposite(Operator.Or, rolesAccessFilters));
        }
    }
}
