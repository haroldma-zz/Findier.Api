using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Managers;
using Findier.Api.Models;
using Findier.Api.Models.Binding;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Models.Identity;
using Findier.Api.Responses;
using Findier.Api.Services;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Controllers
{
    [Authorize, RoutePrefix("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly DtoService _dtoService;
        private readonly AppUserManager _userManager;

        public UsersController(AppUserManager userManager, AppDbContext dbContext, DtoService dtoService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _dtoService = dtoService;
        }
        
        [AllowAnonymous, Route("")]
        [ResponseType(typeof (TokenResponse))]
        public async Task<IHttpActionResult> Post(RegistrationBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var user = new User
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Username
            };

            // Register using password
            var result = await _userManager.CreateAsync(user, model.Password);
            var errorResult = GetErrorResult(result);

            return errorResult ?? Ok(await _userManager.GenerateLocalAccessTokenResponseAsync(user));
        }

        #region Helper

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (result.Succeeded)
            {
                return null;
            }

            if (result.Errors == null)
            {
                return BadRequest();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return ApiBadRequestFromModelState();
        }

        #endregion
    }
}