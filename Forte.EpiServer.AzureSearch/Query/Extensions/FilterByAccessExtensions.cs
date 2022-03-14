using System;
using System.Linq;
using System.Security.Claims;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Query.Extensions
{
    public static class FilterByAccessExtensions
    {
        /// <summary>
        /// Method filters search items by allowed roles and users
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="claimPrincipals"></param>
        /// <returns></returns>
        public static AzureSearchQueryBuilder FilterOnRoleAccess(this AzureSearchQueryBuilder queryBuilder, ClaimsPrincipal claimPrincipals)
        {
            _ = claimPrincipals ?? throw new ArgumentNullException((nameof(claimPrincipals)));
            var currentPrincipalRoles = claimPrincipals.Claims?.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);
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
