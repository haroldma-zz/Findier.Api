using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Findier.Api;
using Findier.Api.Infrastructure;
using Findier.Api.Migrations;
using Findier.Api.Models;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SqlServerTypes;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof (Startup))]

namespace Findier.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Loading SQL Server data types
            Utilities.LoadNativeAssemblies(HostingEnvironment.MapPath("~/bin"));

            // Set database initializer
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());

            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            // Register assembly with autofac builder
            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterApiControllers(assembly);
            builder.RegisterAssemblyModules(assembly);

            // oauth options
            var oauthServerOptions = new OAuthAuthorizationServerOptions
            {
#if DEBUG
                AllowInsecureHttp = true,
#endif
                TokenEndpointPath = new PathString("/api/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(7),
                Provider = new AppAuthorizationServerProvider()
            };
            var bearerOptions = new OAuthBearerAuthenticationOptions();

            builder.Register(p => oauthServerOptions).SingleInstance();
            builder.Register(p => bearerOptions).SingleInstance();

            // Use Autofac as the dependency resolver
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // Register Autofac middleware and then the Autofac web api middleware
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);

            // Configure the oauth server
            app.UseOAuthAuthorizationServer(oauthServerOptions);

            // configure the bearer authentication
            app.UseOAuthBearerAuthentication(bearerOptions);

            // Register swagger
            config
                .EnableSwagger(c =>
                    {
                        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                        c.SingleApiVersion("v1", "Findier");
                        c.DescribeAllEnumsAsStrings();
                        c.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\Findier.Api.XML");
                        c.OperationFilter<SwaggerHandleGeoLocation>();
                    })
                .EnableSwaggerUi(
                    c => { c.InjectJavaScript(typeof (Startup).Assembly, "Findier.Api.Resources.swagger-bearer.js"); });

            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}