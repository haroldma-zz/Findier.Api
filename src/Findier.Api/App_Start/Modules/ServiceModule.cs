using Autofac;
using Findier.Api.Services;

namespace Findier.Api.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DtoService>().InstancePerRequest();
        }
    }
}