using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Managers;
using Findier.Api.Models;
using Findier.Api.Models.Binding;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Models.Identity;
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

        [Route("me/threads/{id}/messages")]
        public async Task<IHttpActionResult> GetThreadMessages(
            string id,
            string after = null,
            string before = null,
            int limit = 20)
        {
            var afterId = after?.FromEncodedId() ?? 0;
            var beforeId = before?.FromEncodedId() ?? 0;
            limit = Math.Min(100, limit);

            var decodedId = id.FromEncodedId();
            var thread = await _dbContext.PostThreads.Include(p => p.Post).FirstOrDefaultAsync(p => p.Id == decodedId);

            if (thread == null || (thread.UserId == User.Id && thread.IsUserDeleted)
                || (thread.Post.UserId == User.Id && thread.IsPostUserDeleted))
            {
                return NotFound();
            }

            if (thread.UserId != User.Id && thread.Post.UserId != User.Id)
            {
                return Unauthorized();
            }
            
            var usingBefore = before != null;
            var messages = await _dbContext.Entry(thread)
                .Collection(p => p.Messages)
                .Query()
                .Include(p => p.User)
                .OrderBy(p => p.Id)
                .Where(p => usingBefore ? p.Id < beforeId : p.Id > afterId)
                .Take(limit)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var firstId = messages.FirstOrDefault()?.Id ?? (afterId == 0 ? beforeId : afterId);
            var lastId = messages.LastOrDefault()?.Id ?? (beforeId == 0 ? afterId : beforeId);
            var hasNext = await _dbContext.Entry(thread)
                .Collection(p => p.Messages)
                .Query()
                .OrderBy(p => p.Id)
                .Where(p => p.Id > firstId)
                .AnyAsync();

            var hasPrev = await _dbContext.Entry(thread)
                .Collection(p => p.Messages)
                .Query()
                .OrderBy(p => p.Id)
                .Where(p => p.Id < lastId)
                .AnyAsync();

            return OkPageData(await _dtoService.CreateListAsync<ThreadMessage, DtoThreadMessage>(messages),
                hasNext,
                hasPrev);
        }

        [Route("me/threads")]
        public async Task<IHttpActionResult> GetThreads()
        {
            var threads = await _dbContext.PostThreads
                .Include(p => p.Post)
                .Where(
                    p =>
                        !((p.UserId == User.Id && p.IsUserDeleted) || (p.Post.UserId == User.Id && p.IsPostUserDeleted)))
                .Where(p => p.UserId == User.Id || p.Post.UserId == User.Id)
                .ToListAsync();

            return OkData(await _dtoService.CreateListAsync<PostThread, DtoThread>(threads));
        }

        [AllowAnonymous, Route("")]
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