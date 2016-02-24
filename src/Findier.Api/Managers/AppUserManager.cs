using System;
using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Models.Identity;
using Findier.Api.Responses;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Findier.Api.Managers
{
    public class AppUserManager : UserManager<User, int>
    {
        private readonly OAuthBearerAuthenticationOptions _oAuthBearerOptions;
        private readonly OAuthAuthorizationServerOptions _oAuthServerOptions;

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
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireDigit = true
            };
        }

        public async Task<TokenResponse> GenerateLocalAccessTokenResponseAsync(User user)
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

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "bearer",
                Username = user.UserName,
                ExpiresIn = (int)tokenExpiration.TotalSeconds
            };

            return tokenResponse;
        }
    }
}