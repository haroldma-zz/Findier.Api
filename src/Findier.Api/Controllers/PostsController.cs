using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.Binding;
using Findier.Api.Models.DataTransfer;
using Findier.Api.Services;

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

        [Route("{id}/comments/{commentId}/downs")]
        public Task<IHttpActionResult> DeleteCommentVoteDown(string id, string commentId)
        {
            return InternalCommentVote(id, commentId, false, true);
        }

        [Route("{id}/comments/{commentId}/ups")]
        public Task<IHttpActionResult> DeleteCommentVoteUp(string id, string commentId)
        {
            return InternalCommentVote(id, commentId, true, true);
        }

        [Route("{id}/downs")]
        public Task<IHttpActionResult> DeletePostVoteDown(string id)
        {
            return InternalPostVote(id, false, true);
        }

        [Route("{id}/ups")]
        public Task<IHttpActionResult> DeletePostVoteUp(string id)
        {
            return InternalPostVote(id, true, true);
        }

        [AllowAnonymous]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetAsync(string id)
        {
            var decodedId = id.FromEncodedId();
            var post = await _dbContext.Posts.FindAsync(decodedId);

            if (post == null || post.DeletedAt != null)
            {
                return NotFound();
            }

            return OkData(await _dtoService.CreateAsync<Post, DtoPost>(post));
        }

        [AllowAnonymous]
        [Route("{id}/comments")]
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

        [GeoLocation]
        public async Task<IHttpActionResult> Post(CreatePostBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedId = model.FinboardId.FromEncodedId();

            var finboard = await _dbContext.Finboards.FirstOrDefaultAsync(p => p.Id == decodedId);

            if (finboard == null || finboard.DeletedAt != null)
            {
                return ApiBadRequest("Finboard doesn't exists.");
            }
            if (finboard.IsArchived)
            {
                return BadRequest("This finboard has been archived.");
            }

            var slug = model.Title.ToUrlSlug();
            var post = new Post
            {
                UserId = User.Id,
                FinboardId = decodedId,
                Title = model.Title,
                Text = model.Text,
                IsNsfw = model.IsNsfw,
                Type = model.Type,
                Price = model.Price,
                Slug = string.IsNullOrEmpty(slug) ? "_" : slug,
                Location = GeoLocation
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            _dbContext.PostVotes.Add(new PostVote
            {
                PostId = post.Id,
                UserId = User.Id,
                IsUp = true
            });
            await _dbContext.SaveChangesAsync();

            return OkData(post.Id.ToEncodedId());
        }

        [Route("{id}/comments")]
        public async Task<IHttpActionResult> PostComment([FromUri] string id, [FromBody] CommentBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
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
                return BadRequest("This post has been archived.");
            }

            var comment = new Comment
            {
                UserId = User.Id,
                PostId = decodedId,
                Text = model.Text
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

            return OkData(await _dtoService.CreateAsync<Comment, DtoComment>(comment));
        }

        [Route("{id}/comments/{commentId}/downs")]
        public Task<IHttpActionResult> PutCommentVoteDown(string id, string commentId)
        {
            return InternalCommentVote(id, commentId, false, false);
        }

        [Route("{id}/comments/{commentId}/ups")]
        public Task<IHttpActionResult> PutCommentVoteUp(string id, string commentId)
        {
            return InternalCommentVote(id, commentId, true, false);
        }

        [Route("{id}/downs")]
        public Task<IHttpActionResult> PutPostVoteDown(string id)
        {
            return InternalPostVote(id, false, false);
        }

        [Route("{id}/ups")]
        public Task<IHttpActionResult> PutPostVoteUp(string id)
        {
            return InternalPostVote(id, true, false);
        }

        #region Helpers

        internal async Task<IHttpActionResult> InternalCommentVote(
            string id,
            string commentId,
            bool isUp,
            bool isRemove)
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedId = id.FromEncodedId();
            var decodedCommentId = commentId.FromEncodedId();

            var comment =
                await _dbContext.Comment.Include(p => p.Post).FirstOrDefaultAsync(p => p.Id == decodedCommentId);

            if (comment == null || comment.DeletedAt != null
                || comment.Post.DeletedAt != null || comment.PostId != decodedId)
            {
                return NotFound();
            }
            if (comment.Post.IsArchived)
            {
                return BadRequest("This thread has been archived.");
            }

            var userVote = await _dbContext.Entry(comment)
                .Collection(p => p.Votes)
                .Query()
                .FirstOrDefaultAsync(p => p.UserId == User.Id);

            if (isRemove && userVote != null && userVote.IsUp == isUp)
            {
                // delete the vote
                _dbContext.CommentVotes.Remove(userVote);
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
                    _dbContext.CommentVotes.Remove(userVote);
                }

                // Create new vote
                _dbContext.CommentVotes.Add(new CommentVote
                {
                    CommentId = comment.Id,
                    UserId = User.Id,
                    IpAddress = ClientIpAddress,
                    IsUp = isUp
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

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
                return BadRequest("This post has been archived.");
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

            return Ok();
        }

        #endregion
    }
}