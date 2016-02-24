using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Findier.Api.Models.Identity;
using Findier.Api.Responses;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Resources;

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
            UserValidator = new AppUserValidator(this)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };
            PasswordValidator = new AppPasswordValidator
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

        public class AppPasswordValidator : PasswordValidator
        {
            public override Task<IdentityResult> ValidateAsync(string item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(item) || item.Length < RequiredLength)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, ApiResources.PasswordTooShort, RequiredLength));
                }
                if (RequireNonLetterOrDigit && item.All(IsLetterOrDigit))
                {
                    errors.Add(ApiResources.PasswordRequireNonLetterOrDigit);
                }
                if (RequireDigit && item.All(c => !IsDigit(c)))
                {
                    errors.Add(ApiResources.PasswordRequireDigit);
                }
                if (RequireLowercase && item.All(c => !IsLower(c)))
                {
                    errors.Add(ApiResources.PasswordRequireLower);
                }
                if (RequireUppercase && item.All(c => !IsUpper(c)))
                {
                    errors.Add(ApiResources.PasswordRequireUpper);
                }
                if (errors.Count == 0)
                {
                    return Task.FromResult(IdentityResult.Success);
                }
                return Task.FromResult(IdentityResult.Failed(string.Join(" ", errors)));
            }
        }

        public class AppUserValidator : UserValidator<User, int>
        {
            private readonly UserManager<User, int> _manager;

            public AppUserValidator(UserManager<User, int> manager) : base(manager)
            {
                _manager = manager;
            }

            /// <summary>
            ///     Validates a user before saving
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public override async Task<IdentityResult> ValidateAsync(User item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                var errors = new List<string>();
                await ValidateUserName(item, errors);
                if (RequireUniqueEmail)
                {
                    await ValidateEmail(item, errors);
                }
                if (errors.Count > 0)
                {
                    return IdentityResult.Failed(errors.ToArray());
                }
                return IdentityResult.Success;
            }

            // make sure email is not empty, valid, and unique
            private async Task ValidateEmail(User user, List<string> errors)
            {
                var email = user.Email;
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add(ApiResources.EmptyEmail);
                    return;
                }
                try
                {
                    var m = new MailAddress(email);
                }
                catch (FormatException)
                {
                    errors.Add(ApiResources.InvalidEmail);
                    return;
                }
                var owner = await _manager.FindByEmailAsync(email);
                if (owner != null && !EqualityComparer<int>.Default.Equals(owner.Id, user.Id))
                {
                    errors.Add(ApiResources.DuplicateEmail);
                }
            }

            private async Task ValidateUserName(User user, List<string> errors)
            {
                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    errors.Add(ApiResources.EmptyUsername);
                }
                else if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, @"^[A-Za-z0-9@_\.]+$"))
                {
                    // If any characters are not letters or digits, its an illegal user name
                    errors.Add(ApiResources.InvalidUsername);
                }
                else
                {
                    var owner = await _manager.FindByNameAsync(user.UserName);
                    if (owner != null && !EqualityComparer<int>.Default.Equals(owner.Id, user.Id))
                    {
                        errors.Add(ApiResources.DuplicateUsername);
                    }
                }
            }
        }
    }
}