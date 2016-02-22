using System.Threading.Tasks;
using Findier.Api.Models;
using Findier.Api.Models.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Findier.Api.Managers
{
    public class AppUserStore : UserStore<User, AppRole, int, AppUserLogin, AppUserRole, AppUserClaim>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public AppUserStore(AppDbContext context) : base(context)
        {
        }

        public override Task CreateAsync(User user)
        {
            user.NormalizedUserName = user.UserName.ToLower();
            user.NormalizedEmail = user.Email.ToLower();
            return base.CreateAsync(user);
        }

        public override Task<User> FindByEmailAsync(string email)
        {
            email = email.ToLower();
            return GetUserAggregateAsync(u => u.NormalizedEmail == email);
        }

        public override Task<User> FindByNameAsync(string userName)
        {
            userName = userName.ToLower();
            return GetUserAggregateAsync(u => u.NormalizedUserName == userName);
        }
    }
}