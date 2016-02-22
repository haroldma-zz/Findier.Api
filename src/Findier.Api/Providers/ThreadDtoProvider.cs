using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Findier.Api.Extensions;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Providers
{
    public class ThreadDtoProvider : IDtoProvider<PostThread, DtoThread>
    {
        private readonly AppDbContext _dbContext;

        public ThreadDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoThread> CreateAsync(PostThread entry)
        {
            var userId = HttpContext.Current.User.Identity.GetUserIdDecoded();
            string username;
            string postUsername;

            if (entry.User != null)
            {
                username = entry.User.UserName;
            }
            else
            {
                username = await _dbContext.Entry(entry).Reference(p => p.User)
                    .Query()
                    .Select(p => p.UserName)
                    .SingleAsync();
            }

            if (entry.Post != null)
            {
                await _dbContext.Entry(entry).Reference(p => p.Post).LoadAsync();
            }

            if (entry.Post?.User != null)
            {
                postUsername = entry.Post.User.UserName;
            }
            else if (entry.Post?.UserId == userId)
            {
                postUsername = HttpContext.Current.User.Identity.GetUserName();
            }
            else
            {
                postUsername = await _dbContext.Entry(entry.Post)
                    .Reference(p => p.User)
                    .Query()
                    .Select(p => p.UserName)
                    .SingleAsync();
            }

            if (entry.Post?.UserId != userId)
            {
                username = postUsername;
            }

            var unread = await _dbContext.Entry(entry)
                .Collection(p => p.Messages)
                .Query()
                .Where(p => !p.IsRead && p.UserId != userId)
                .CountAsync();

            return new DtoThread(entry)
            {
                Unread = unread,
                User = username,
                Post = new DtoBasicPost(entry.Post, postUsername)
            };
        }
    }
}