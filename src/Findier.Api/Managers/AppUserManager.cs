using System;
using System.Globalization;
using System.Threading.Tasks;
using Findier.Api.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;

namespace Findier.Api.Managers
{
    public class AppUserManager : UserManager<User, int>
    {
        private readonly OAuthAuthorizationServerOptions _oAuthServerOptions;
        private readonly OAuthBearerAuthenticationOptions _oAuthBearerOptions;

        public AppUserManager(
            AppUserStore store,
            AppClaimsFactory claimsFactory,
            OAuthAuthorizationServerOptions oAuthServerOptions,
            OAuthBearerAuthenticationOptions oAuthBearerOptions)
            : base(store)
        {
            _oAuthServerOptions = oAuthServerOptions;
            _oAuthBearerOptions = oAuthBearerOptions;
            ClaimsIdentityFactory = claimsFactory;
            UserValidator = new UserValidator<User, int>(this)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };
        }

        public async Task<JObject> GenerateLocalAccessTokenResponseAsync(User user)
        {
            var tokenExpiration = _oAuthServerOptions.AccessTokenExpireTimeSpan;

            var identity = await CreateIdentityAsync(user, "JWT");

            var props = new AuthenticationProperties
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration)
            };
            var ticket = new AuthenticationTicket(identity, props);

            var accessToken = _oAuthBearerOptions.AccessTokenFormat.Protect(ticket);

            var tokenResponse = new JObject(
                new JProperty("access_token", accessToken),
                new JProperty("token_type", "bearer"),
                new JProperty("username", user.UserName),
                new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture))
                );

            return tokenResponse;
        }
    }
}