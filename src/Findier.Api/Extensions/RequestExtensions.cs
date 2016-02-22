using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net.Http;
using Findier.Api.Helpers;

namespace Findier.Api.Extensions
{
    internal static class RequestExtensions
    {
        public static DbGeography GetGeoLocation(this HttpRequestMessage request)
        {
            IEnumerable<string> value;
            if (!request.Headers.TryGetValues("X-Geo-Location", out value))
            {
                return null;
            }
            var geo = value.FirstOrDefault();
            DbGeography location;
            GeoHelper.TryCreatePoint(geo, out location);
            return location;
        }
    }
}