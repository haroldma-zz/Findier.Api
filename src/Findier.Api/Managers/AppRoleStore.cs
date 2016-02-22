using Findier.Api.Models;
using Findier.Api.Models.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Findier.Api.Managers
{
    public class AppRoleStore : RoleStore<AppRole, int, AppUserRole>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public AppRoleStore(AppDbContext context) : base(context)
        {
        }
    }
}