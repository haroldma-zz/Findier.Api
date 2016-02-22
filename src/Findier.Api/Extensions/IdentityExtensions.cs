using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Extensions
{
    public static class UserExtensions
    {
        public static int GetUserIdDecoded(this IIdentity identity)
        {
            return identity.GetUserId().FromEncodedId();
        }
    }
}