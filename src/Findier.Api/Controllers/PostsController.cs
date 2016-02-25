using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
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
using Resources;
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
                return ApiBadRequest(ApiResources.CategoryArchived);
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
        ///     Gets a feed of posts from all categories.
        /// </summary>
        /// <param name="sort">The sort.</param>
        /// <param name="offset">The offset (paging).</param>
        /// <param name="limit">The limit (paging).</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof (FindierResponse<FindierPageData<DtoPlainPost>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetPosts(PostSort sort = PostSort.New, int offset = 0, int limit = 20)
        {
            offset = Math.Max(0, offset);
            limit = Math.Min(100, limit);

            var max = await _dbContext.Posts
                .ExcludeDeleted()
                .CountAsync();

            List<Post> posts;
            if (sort == PostSort.Hot)
            {
                // calculating hotness needs a bit of a more complex query
                var epochTime = new DateTime(1970, 1, 1);
                posts = await _dbContext.Posts
                    .ExcludeDeleted()
                    .Select(p => new
                    {
                        Post = p,
                        Score = p.Votes.Count(m => m.IsUp) - p.Votes.Count(m => !m.IsUp)
                    })
                    .Select(p => new
                            {
                                p.Post,
                                Order = (double)(SqlFunctions.Log((double)Math.Abs(p.Score))/ SqlFunctions.Log(10f)),
                                Sign = p.Score > 0 ? 1 : (p.Score < 0 ? -1 : 0),
                                Seconds = (double)DbFunctions.DiffSeconds(epochTime, p.Post.CreatedAt) - 1456272000
                    })
                            .Select(p => new
                            {
                                p.Post,
                                Hotness = Math.Round(p.Sign * p.Order + p.Seconds / 45000, 7),
                                p.Seconds
                            } )
                    .OrderByDescending(p => p.Hotness)
                    .Skip(offset)
                    .Take(limit)
                    .Select(p => p.Post)
                    .ToListAsync();
            }
            else
            {
                Expression<Func<Post, int>> sortClause = p => p.Id;

                if (sort == PostSort.Top)
                {
                    sortClause = p => p.Votes.Count(m => m.IsUp) - p.Votes.Count(m => !m.IsUp);
                }

                posts = await _dbContext.Posts
                    .ExcludeDeleted()
                    .OrderByDescending(sortClause)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
            }

            return OkPageData(await _dtoService.CreateListAsync<Post, DtoPlainPost>(posts), offset + limit < max);
        }

        /// <summary>
        ///     Create a new post in the specified category.
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

            var decodedId = newPost.CategoryId.FromEncodedId();

            var category = await _dbContext.Categories.FirstOrDefaultAsync(p => p.Id == decodedId);

            if (category == null)
            {
                return ApiBadRequest(ApiResources.CategoryNotFound);
            }
            if (category.IsArchived)
            {
                return ApiBadRequest(ApiResources.CategoryArchived);
            }

            if (string.IsNullOrWhiteSpace(newPost.Email)
                && string.IsNullOrWhiteSpace(newPost.PhoneNumber))
            {
                return ApiBadRequest(ApiResources.ContactMethodRequired);
            }

            var slug = newPost.Title.ToUrlSlug();
            var post = new Post
            {
                UserId = User.Id,
                CategoryId = decodedId,
                Title = newPost.Title,
                Text = newPost.Text,
                IsNsfw = category.IsNsfw || newPost.IsNsfw,
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
                return ApiBadRequest(ApiResources.PostArchived);
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
        ///     Updates the specified post.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <param name="updatedPost">The updated post.</param>
        /// <returns></returns>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof (FindierErrorResponse))]
        public async Task<IHttpActionResult> Put([FromUri] string id, [FromBody] UpdatedPost updatedPost)
        {
            var decodedId = id.FromEncodedId();
            var post = await _dbContext.Posts.FindAsync(decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }

            if (post.UserId != User.Id)
            {
                return ApiBadRequest(ApiResources.PostNoOwnership);
            }

            if (updatedPost.Text != null)
            {
                post.Text = updatedPost.Text;
            }

            if (updatedPost.Type != null)
            {
                post.Type = updatedPost.Type.Value;
                if (post.Type != PostType.Fixed)
                {
                    post.Price = 0;
                }
            }

            if (updatedPost.IsNsfw != null && !post.IsNsfw)
            {
                post.IsNsfw = updatedPost.IsNsfw.Value;
            }

            if (post.Type == PostType.Fixed)
            {
                if (updatedPost.Price != null)
                {
                    post.Price = Math.Max(updatedPost.Price.Value, 1);
                }
                else if (post.Price < 1)
                {
                    return ApiBadRequest(ApiResources.PostIncludePrice);
                }
            }

            if (updatedPost.Email != null)
            {
                if (!string.IsNullOrWhiteSpace(updatedPost.Email))
                {
                    post.Email = updatedPost.Email;
                }
                else
                {
                    if ((updatedPost.PhoneNumber != null && string.IsNullOrWhiteSpace(updatedPost.PhoneNumber))
                        || post.PhoneNumber == null)
                    {
                        return ApiBadRequest(ApiResources.ContactMethodRequired);
                    }
                    post.Email = null;
                }
            }

            if (updatedPost.PhoneNumber != null)
            {
                if (!string.IsNullOrWhiteSpace(updatedPost.PhoneNumber))
                {
                    post.PhoneNumber = updatedPost.PhoneNumber;
                }
                else
                {
                    if (post.Email == null)
                    {
                        return ApiBadRequest(ApiResources.ContactMethodRequired);
                    }
                    post.PhoneNumber = null;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
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
                return ApiBadRequest(ApiResources.PostArchived);
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