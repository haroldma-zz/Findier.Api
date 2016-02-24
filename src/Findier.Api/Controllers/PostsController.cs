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
using Findier.Api.Models.Binding;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Responses;
using Findier.Api.Services;
using Swashbuckle.Swagger.Annotations;

namespace Findier.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/posts")]
    public class PostsController : BaseApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly DtoService _dtoService;

        public PostsController(AppDbContext dbContext, DtoService dtoService)
        {
            _dbContext = dbContext;
            _dtoService = dtoService;
        }

        /// <summary>
        ///     Deletes the specified post.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof (FindierErrorResponse))]
        public async Task<IHttpActionResult> Delete(string id)
        {
            var decodedId = id.FromEncodedId();
            var post = await _dbContext.Posts.FindAsync(decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }

            if (post.UserId != User.Id)
            {
                return ApiBadRequest("This post doesn't belong to you.");
            }

            post.Text = null;
            post.PhoneNumber = null;
            post.Email = null;
            post.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        ///     Deletes the user's post downvote.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [Route("{id}/downs")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was deleted.")]
        [SwaggerResponse(HttpStatusCode.OK, "No vote from user.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> DeletePostDownvote(string id)
        {
            return InternalPostVote(id, false, true);
        }

        /// <summary>
        ///     Deletes the user's post upvote.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [Route("{id}/ups")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was deleted.")]
        [SwaggerResponse(HttpStatusCode.OK, "No vote from user.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> DeletePostUpvote(string id)
        {
            return InternalPostVote(id, true, true);
        }

        /// <summary>
        ///     Gets the specified post.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("{id}")]
        [ResponseType(typeof (FindierResponse<DtoPost>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(string id)
        {
            var decodedId = id.FromEncodedId();
            var post = await _dbContext.Posts.FindAsync(decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }

            return OkData(await _dtoService.CreateAsync<Post, DtoPost>(post));
        }

        /// <summary>
        ///     Gets the post's comments.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <param name="offset">The offset (pagination).</param>
        /// <param name="limit">The limit (pagination).</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("{id}/comments")]
        [ResponseType(typeof (FindierResponse<FindierPageData<DtoComment>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetComments(string id, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);
            var decodedId = id.FromEncodedId();

            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == decodedId);

            if (post == null)
            {
                return NotFound();
            }

            var max = await _dbContext.Entry(post)
                .Collection(p => p.Comments)
                .Query()
                .CountAsync();

            var comments = await _dbContext.Entry(post)
                .Collection(p => p.Comments)
                .Query()
                .OrderByDescending(p => p.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return OkPageData(await _dtoService.CreateListAsync<Comment, DtoComment>(comments), offset + limit < max);
        }

        /// <summary>
        ///     Create a new post in the specified finboard.
        /// </summary>
        /// <param name="newPost">The new post.</param>
        /// <returns></returns>
        [GeoLocation]
        [Route("")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Created,
            "Returns the id of the created post.",
            typeof (FindierResponse<string>))]
        public async Task<IHttpActionResult> Post(NewPost newPost)
        {
            if (newPost == null || !ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedId = newPost.FinboardId.FromEncodedId();

            var finboard = await _dbContext.Finboards.FirstOrDefaultAsync(p => p.Id == decodedId);

            if (finboard == null || finboard.DeletedAt != null)
            {
                return ApiBadRequest("Finboard doesn't exists.");
            }
            if (finboard.IsArchived)
            {
                return ApiBadRequest("This finboard has been archived.");
            }

            if (string.IsNullOrWhiteSpace(newPost.Email)
                && string.IsNullOrWhiteSpace(newPost.PhoneNumber))
            {
                return ApiBadRequest("Please enter at least one contact method.");
            }

            var slug = newPost.Title.ToUrlSlug();
            var post = new Post
            {
                UserId = User.Id,
                FinboardId = decodedId,
                Title = newPost.Title,
                Text = newPost.Text,
                IsNsfw = newPost.IsNsfw,
                Type = newPost.Type,
                Price = Math.Max(newPost.Price, newPost.Type == PostType.Fixed ? 1 : 0),
                Slug = string.IsNullOrEmpty(slug) ? "_" : slug,
                Location = GeoLocation,
                PhoneNumber = newPost.PhoneNumber,
                Email = newPost.Email
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            _dbContext.PostVotes.Add(new PostVote
            {
                PostId = post.Id,
                UserId = User.Id,
                IpAddress = ClientIpAddress,
                IsUp = true
            });
            await _dbContext.SaveChangesAsync();

            return CreatedData(post.Id.ToEncodedId());
        }

        /// <summary>
        ///     Creates a new comment in the specified post.
        /// </summary>
        /// <param name="id">The id of the post.</param>
        /// <param name="newComment">The new comment.</param>
        /// <returns></returns>
        [Route("{id}/comments")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Created, "Returns the id of the created comment.",
            typeof (FindierResponse<string>))]
        public async Task<IHttpActionResult> PostComment([FromUri] string id, [FromBody] NewComment newComment)
        {
            if (newComment == null || !ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedId = id.FromEncodedId();

            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }
            if (post.IsArchived)
            {
                return ApiBadRequest("This post has been archived.");
            }

            var comment = new Comment
            {
                UserId = User.Id,
                PostId = decodedId,
                Text = newComment.Text
            };
            _dbContext.Comment.Add(comment);

            if (await _dbContext.SaveChangesAsync() == 0)
            {
                return InternalServerError();
            }

            // auto-upvote comment
            _dbContext.CommentVotes.Add(new CommentVote
            {
                CommentId = comment.Id,
                UserId = User.Id,
                IpAddress = ClientIpAddress,
                IsUp = true
            });
            await _dbContext.SaveChangesAsync();
            return CreatedData(comment.Id.ToEncodedId());
        }

        /// <summary>
        ///     Puts a downvote to the post.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [Route("{id}/downs")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was casted.")]
        [SwaggerResponse(HttpStatusCode.OK, "The vote was already casted.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> PutPostDownvote(string id)
        {
            return InternalPostVote(id, false, false);
        }

        /// <summary>
        ///     Puts an upvote to the post.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns></returns>
        [Route("{id}/ups")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was casted.")]
        [SwaggerResponse(HttpStatusCode.OK, "The vote was already casted.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> PutPostUpvote(string id)
        {
            return InternalPostVote(id, true, false);
        }

        #region Helpers

        internal async Task<IHttpActionResult> InternalPostVote(
            string id,
            bool isUp,
            bool isRemove)
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedId = id.FromEncodedId();

            var post = await _dbContext.Posts.FindAsync(decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }
            if (post.IsArchived)
            {
                return ApiBadRequest("This post has been archived.");
            }

            var userVote = await _dbContext.Entry(post)
                .Collection(p => p.Votes)
                .Query()
                .FirstOrDefaultAsync(p => p.UserId == User.Id);

            if (isRemove && userVote != null && userVote.IsUp == isUp)
            {
                // delete the vote
                _dbContext.PostVotes.Remove(userVote);
            }
            else if (!isRemove)
            {
                if (userVote != null)
                {
                    // vote already cast
                    if (userVote.IsUp == isUp)
                    {
                        return Ok();
                    }

                    // delete old opposite vote
                    _dbContext.PostVotes.Remove(userVote);
                }

                // Create new vote
                _dbContext.PostVotes.Add(new PostVote
                {
                    PostId = post.Id,
                    UserId = User.Id,
                    IpAddress = ClientIpAddress,
                    IsUp = isUp
                });
            }

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}