using System.Linq;
using Findier.Api.Models;

namespace Findier.Api.Extensions
{
    public static class EntityExtensions
    {
        public static IQueryable<TSource> ExcludeDeleted<TSource>(
            this IQueryable<TSource> source) where TSource : DbEntryDeletable
        {
            return source.Where(p => p.DeletedAt == null);
        }
    }
}