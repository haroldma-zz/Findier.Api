using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class ThreadMessageDtoProvider : IDtoProvider<ThreadMessage, DtoThreadMessage>
    {
        private readonly AppDbContext _dbContext;

        public ThreadMessageDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoThreadMessage> CreateAsync(ThreadMessage entry)
        {
            string username;

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
            return new DtoThreadMessage(entry, username);
        }
    }
}