using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Responses;
using Resources;
using Swashbuckle.Swagger.Annotations;

namespace Findier.Api.Controllers
{
    [RoutePrefix("api/comments")]
    public class CommentsController : BaseApiController
    {
        private readonly AppDbContext _dbContext;

        public CommentsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        ///     Deletes the user's comment downvote.
        /// </summary>
        /// <param name="id">The comment id.</param>
        /// <returns></returns>
        [Route("{id}/downs")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was deleted.")]
        [SwaggerResponse(HttpStatusCode.OK, "No vote from user.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> DeleteCommentDownvote(string id)
        {
            return InternalCommentVote(id, false, true);
        }

        /// <summary>
        ///     Deletes the user's comment upvote.
        /// </summary>
        /// <param name="id">The comment id.</param>
        /// <returns></returns>
        [Route("{id}/ups")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was deleted.")]
        [SwaggerResponse(HttpStatusCode.OK, "No vote from user.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> DeleteCommentUpvote(string id)
        {
            return InternalCommentVote(id, true, true);
        }

        /// <summary>
        ///     Puts a downvote to the comment.
        /// </summary>
        /// <param name="id">The comment id.</param>
        /// <returns></returns>
        [Route("{id}/downs")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was casted.")]
        [SwaggerResponse(HttpStatusCode.OK, "The vote was already casted.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> PutCommentDownvote(string id)
        {
            return InternalCommentVote(id, false, false);
        }

        /// <summary>
        ///     Puts an upvote to the comment.
        /// </summary>
        /// <param name="id">The comment id.</param>
        /// <returns></returns>
        [Route("{id}/ups")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent, "The vote was casted.")]
        [SwaggerResponse(HttpStatusCode.OK, "The vote was already casted.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof (FindierErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Task<IHttpActionResult> PutCommentUpvote(string id)
        {
            return InternalCommentVote(id, true, false);
        }

        #region Helpers

        internal async Task<IHttpActionResult> InternalCommentVote(
            string commentId,
            bool isUp,
            bool isRemove)
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var decodedCommentId = commentId.FromEncodedId();

            var comment =
                await _dbContext.Comment.Include(p => p.Post).FirstOrDefaultAsync(p => p.Id == decodedCommentId);

            if (comment == null || comment.DeletedAt != null
                || comment.Post.DeletedAt != null)
            {
                return NotFound();
            }
            if (comment.Post.IsArchived)
            {
                return ApiBadRequest(ApiResources.PostArchived);
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

            return NoContent();
        }

        #endregion
    }
}