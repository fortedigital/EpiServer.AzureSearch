using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.Helpers;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch.Query.Extensions
{
    public static class FilterByAccessExtensions
    {
        /// <summary>
        /// Method filters search items by allowed roles and users
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static AzureSearchQueryBuilder FilterOnRoleAccess(this AzureSearchQueryBuilder queryBuilder, IPrincipal principal)
        {
            _ = principal ?? throw new ArgumentNullException(nameof(principal));

            var rolesAccessFilters = GetRoles(principal)
                .Select(
                    roleName => new AzureSearchQueryFilter(
                        nameof(ContentDocument.AccessRoles),
                        ComparisonExpression.Eq,
                        roleName) { GroupingExpression = GroupingExpression.Any });

            return queryBuilder.Filter(new FilterComposite(Operator.Or, rolesAccessFilters));
        }

        /// <summary>
        /// Method filters search items by allowed roles for current user.
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <returns></returns>
        public static AzureSearchQueryBuilder FilterOnRoleAccessForCurrentPrincipal(this AzureSearchQueryBuilder queryBuilder)
        {
            var currentPrincipal = ServiceLocator.Current.TryGetExistingInstance<IPrincipalAccessor>(out var instance)
                ? instance.Principal
                : PrincipalFallbackProvider.Current;

            return queryBuilder.FilterOnRoleAccess(currentPrincipal);
        }

        private static IEnumerable<string> GetRoles(IPrincipal principal)
        {
            return principal is ClaimsPrincipal claimsPrincipal
                ? claimsPrincipal.Identities.SelectMany(
                    i => i.Claims
                        .Where(c => c.Type == i.RoleClaimType)
                        .Select(c => c.Value))
                : Enumerable.Empty<string>();
        }
    }
}
