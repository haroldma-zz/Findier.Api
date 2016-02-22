using Findier.Api.Models.Identity;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Managers
{
    public class AppRoleManager : RoleManager<AppRole, int>
    {
        public AppRoleManager(AppRoleStore roleStore)
            : base(roleStore)
        {
        }
    }
}