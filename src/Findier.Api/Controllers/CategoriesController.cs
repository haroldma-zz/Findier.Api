using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Findier.Api.Enums;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Responses;
using Findier.Api.Services;
using Swashbuckle.Swagger.Annotations;

namespace Findier.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/categories")]
    public class CategoriesController : BaseApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly DtoService _dtoService;

        public CategoriesController(AppDbContext dbContext, DtoService dtoService)
        {
            _dbContext = dbContext;
            _dtoService = dtoService;
        }

        /// <summary>
        ///     Gets a feed of the country's categories.
        /// </summary>
        /// <param name="country">The country.</param>
        /// <param name="offset">The offset (paging).</param>
        /// <param name="limit">The limit (paging).</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("")]
        [ResponseType(typeof (FindierResponse<FindierPageData<DtoCategory>>))]
        public async Task<IHttpActionResult> Get(Country country, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);

            var max = await _dbContext.Categories.CountAsync();
            var categories = await _dbContext.Categories
                .Where(p => p.Country == country)
                .OrderByDescending(p => p.Posts.Count)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return OkPageData(await _dtoService.CreateListAsync<Category, DtoCategory>(categories), offset + limit < max);
        }

        /// <summary>
        ///     Gets a feed of posts of the specified categorie.
        /// </summary>
        /// <param name="id">The categorie id.</param>
        /// <param name="offset">The offset (paging).</param>
        /// <param name="limit">The limit (paging).</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("{id}/posts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof (FindierResponse<FindierPageData<DtoPlainPost>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetPosts(string id, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);

            var decodedId = id.FromEncodedId();

            var category = await _dbContext.Categories.FindAsync(decodedId);

            if (category == null)
            {
                return NotFound();
            }

            var max = await _dbContext.Entry(category)
                .Collection(p => p.Posts)
                .Query()
                .ExcludeDeleted()
                .CountAsync();

            var posts = await _dbContext.Entry(category)
                .Collection(p => p.Posts)
                .Query()
                .ExcludeDeleted()
                .OrderByDescending(p => p.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return OkPageData(await _dtoService.CreateListAsync<Post, DtoPlainPost>(posts), offset + limit < max);
        }
    }
}