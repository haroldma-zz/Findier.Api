using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class PlainPostDtoProvider : IDtoProvider<Post, DtoPlainPost>
    {
        private readonly AppDbContext _dbContext;

        public PlainPostDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoPlainPost> CreateAsync(Post entry)
        {
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

            return new DtoPlainPost(entry, username, ups, downs);
        }
    }
}