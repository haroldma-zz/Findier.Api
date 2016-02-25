using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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

        public static IQueryable<TSource> OrderByHotness<TSource>(
            this IQueryable<TSource> source) where TSource : Post
        {
            var epochTime = new DateTime(1970, 1, 1);
            return source.Select(p => new
            {
                Post = p,
                Score = p.Votes.Count(m => m.IsUp) - p.Votes.Count(m => !m.IsUp)
            }).Select(p => new
            {
                p.Post,
                Order = (double)(SqlFunctions.Log((double)Math.Abs(p.Score)) / SqlFunctions.Log(10f)),
                Sign = p.Score > 0 ? 1 : (p.Score < 0 ? -1 : 0),
                Seconds = (double)DbFunctions.DiffSeconds(epochTime, p.Post.CreatedAt) - 1456272000
            })
                .Select(p => new
                {
                    p.Post,
                    Hotness = Math.Round(p.Sign * p.Order + p.Seconds / 45000, 7)
                })
                .OrderByDescending(p => p.Hotness)
                .Select(p => p.Post);
        }
    }
}