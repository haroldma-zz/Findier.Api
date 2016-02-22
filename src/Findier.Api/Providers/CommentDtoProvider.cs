using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class CommentDtoProvider : IDtoProvider<Comment, DtoComment>
    {
        private readonly AppDbContext _dbContext;

        public CommentDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoComment> CreateAsync(Comment entry)
        {
            int ups;
            int downs;
            string username;
            bool isOp;

            if (entry.Post == null)
            {
                isOp = await _dbContext.Entry(entry)
                    .Reference(p => p.Post)
                    .Query()
                    .Select(p => p.UserId)
                    .SingleAsync() == entry.UserId;
            }
            else
            {
                isOp = entry.Post.UserId == entry.UserId;
            }

            if (entry.User == null)
            {
                username = await _dbContext.Entry(entry)
                    .Reference(p => p.User)
                    .Query()
                    .Select(p => p.UserName)
                    .SingleAsync();
            }
            else
            {
                username = entry.User.UserName;
            }

            if (entry.Votes == null)
            {
                ups = await _dbContext.Entry(entry).Collection(p => p.Votes).Query().CountAsync(p => p.IsUp);
                downs = await _dbContext.Entry(entry).Collection(p => p.Votes).Query().CountAsync(p => !p.IsUp);
            }
            else
            {
                ups = entry.Votes.Count(p => p.IsUp);
                downs = entry.Votes.Count(p => !p.IsUp);
            }

            return new DtoComment(entry, username, isOp, ups, downs);
        }
    }
}