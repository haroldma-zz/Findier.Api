using System.Threading.Tasks;

namespace Findier.Api.Infrastructure
{
    public interface IDtoProvider<in T, TT>
    {
        Task<TT> CreateAsync(T entry);
    }
}