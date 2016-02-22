using System.Threading.Tasks;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class FinboardDtoProvider : IDtoProvider<Finboard, DtoFinboard>
    {
        public Task<DtoFinboard> CreateAsync(Finboard entry)
        {
            return Task.FromResult(new DtoFinboard(entry));
        }
    }
}