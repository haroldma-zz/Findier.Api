using Microsoft.AspNet.Identity.EntityFramework;

namespace Findier.Api.Models.Identity
{
    public class AppRole : IdentityRole<int, AppUserRole>
    {
    }
}