using System.Linq;
using Findier.Api.Infrastructure;
using HashidsNet;

namespace Findier.Api.Extensions
{
    public static class HashidsExtensions
    {
        private static readonly Hashids Hashids = new Hashids(AppConfigurations.HashidsSalt);

        public static int FromEncodedId(this string id)
        {
            try
            {
                return Hashids.Decode(id).FirstOrDefault();
            }
            catch
            {
                return -1;
            }
        }

        public static string ToEncodedId(this int key)
        {
            return Hashids.Encode(key);
        }
    }
}