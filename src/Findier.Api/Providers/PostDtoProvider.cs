using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class PostDtoProvider : IDtoProvider<Post, DtoPost>
    {
        private readonly AppDbContext _dbContext;

        public PostDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoPost> CreateAsync(Post entry)
        {
            string finboard;
            string username;
            int ups;
            int downs;

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

            if (entry.Finboard == null)
            {
                finboard = await _dbContext.Entry(entry)
                    .Reference(p => p.Finboard)
                    .Query()
                    .Select(p => p.Title)
                    .SingleAsync();
            }
            else
            {
                finboard = entry.Finboard.Title;
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

            return new DtoPost(entry, username, ups, downs, new DtoPlainFinboard(entry.FinboardId.ToEncodedId(), finboard));
        }
    }
}