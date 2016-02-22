using System.Security.Claims;
using System.Threading.Tasks;
using Findier.Api.Extensions;
using Findier.Api.Models.Identity;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Managers
{
    public class AppClaimsFactory : ClaimsIdentityFactory<User, int>
    {
        public override string ConvertIdToString(int key)
        {
            return key.ToEncodedId();
        }

        public override async Task<ClaimsIdentity> CreateAsync(
            UserManager<User, int> manager,
            User user,
            string authenticationType)
        {
            var claims = await base.CreateAsync(manager, user, authenticationType);
            return claims;
        }
    }
}