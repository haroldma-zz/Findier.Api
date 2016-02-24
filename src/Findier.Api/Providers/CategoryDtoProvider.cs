using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class CategoryDtoProvider : IDtoProvider<Category, DtoCategory>
    {
        public Task<DtoCategory> CreateAsync(Category entry)
        {
            return Task.FromResult(new DtoCategory(entry));
        }
    }
}