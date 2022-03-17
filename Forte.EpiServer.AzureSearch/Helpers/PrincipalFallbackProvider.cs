using System.Security.Principal;
using System.Threading;

namespace Forte.EpiServer.AzureSearch.Helpers
{
    public class PrincipalFallbackProvider
    {
        public static IPrincipal Current => Thread.CurrentPrincipal ?? AnonymousPrincipal;
        public static IPrincipal AnonymousPrincipal => new GenericPrincipal(new GenericIdentity(string.Empty), null);
    }
}
