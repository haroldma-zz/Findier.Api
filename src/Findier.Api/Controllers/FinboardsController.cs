using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Findier.Api.Enums;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Services;

namespace Findier.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/finboards")]
    public class FinboardsController : BaseApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly DtoService _dtoService;

        public FinboardsController(AppDbContext dbContext, DtoService dtoService)
        {
            _dbContext = dbContext;
            _dtoService = dtoService;
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Get(Country country, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);

            var max = await _dbContext.Finboards.CountAsync();
            var finboards = await _dbContext.Finboards
                .Where(p => p.Country == country)
                .OrderBy(p => p.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return OkPageData(await _dtoService.CreateListAsync<Finboard, DtoFinboard>(finboards), offset + limit < max);
        }

        [AllowAnonymous]
        [Route("{id}/posts")]
        public async Task<IHttpActionResult> GetPosts(string id, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);

            var decodedId = id.FromEncodedId();

            var finboard = await _dbContext.Finboards.FindAsync(decodedId);

            if (finboard == null || finboard.DeletedAt != null)
            {
                return NotFound();
            }

            var max = await _dbContext.Entry(finboard)
                .Collection(p => p.Posts)
                .Query().CountAsync();

            var posts = await _dbContext.Entry(finboard)
                .Collection(p => p.Posts)
                .Query()
                .OrderByDescending(p => p.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return OkPageData(await _dtoService.CreateListAsync<Post, DtoPlainPost>(posts), offset + limit < max);
        }
    }
}