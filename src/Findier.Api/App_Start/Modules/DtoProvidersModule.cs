using Autofac;
using Findier.Api.Infrastructure;

namespace Findier.Api.Modules
{
    public class DtoProvidersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Every DTO provider implement this type
            var dtoProviderInterface = typeof (IDtoProvider<,>);

            builder.RegisterAssemblyTypes(dtoProviderInterface.Assembly)
                .AsClosedTypesOf(dtoProviderInterface)
                .AsImplementedInterfaces();
        }
    }
}