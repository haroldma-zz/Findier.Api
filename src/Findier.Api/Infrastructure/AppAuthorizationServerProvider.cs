using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Autofac.Integration.Owin;
using Findier.Api.Controllers;
using Findier.Api.Managers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Resources;

namespace Findier.Api.Infrastructure
{
    public class AppAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var lifetimeScope = context.OwinContext.GetAutofacLifetimeScope();
            var userManager = lifetimeScope.Resolve<AppUserManager>();

            var user = await userManager.FindAsync(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError(ApiResources.UsernamePasswordIncorrect);
                return;
            }

            var identity = await userManager.CreateIdentityAsync(user, "JWT");
            var ticket = new AuthenticationTicket(identity,
                new AuthenticationProperties(new Dictionary<string, string>
                {
                    { "Username", user.UserName }
                }));

            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (var property in context.Properties.Dictionary.Where(property => !property.Key.StartsWith(".")))
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult(0);
        }
    }
}