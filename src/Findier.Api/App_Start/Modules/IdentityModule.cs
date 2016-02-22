using Autofac;
using Findier.Api.Managers;
using Findier.Api.Models;

namespace Findier.Api.Modules
{
    public class IdentityModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppDbContext>().InstancePerRequest();
            builder.RegisterType<AppClaimsFactory>().InstancePerRequest();
            builder.RegisterType<AppUserStore>().InstancePerRequest();
            builder.RegisterType<AppRoleStore>().InstancePerRequest();
            builder.RegisterType<AppRoleManager>().InstancePerRequest();
            builder.RegisterType<AppUserManager>().InstancePerRequest();
        }
    }
}