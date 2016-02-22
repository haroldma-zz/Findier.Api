using System;
using System.Data.Entity.Spatial;

namespace Findier.Api.Helpers
{
    public static class GeoHelper
    {
        /// <summary>
        ///     Create a GeoLocation point based on latitude and longitude
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns></returns>
        public static DbGeography CreatePoint(double latitude, double longitude, int coordinateSystemId = 4326)
        {
            var text = $"POINT({longitude} {latitude})";
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.PointFromText(text, coordinateSystemId);
        }

        /// <summary>
        ///     Create a GeoLocation point based on latitude and longitude
        /// </summary>
        /// <param name="latitudeLongitude">
        ///     String should be two values either single comma or space delimited
        ///     45.710030, -121.516153
        ///     45.710030 -121.516153
        /// </param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns></returns>
        public static DbGeography CreatePoint(string latitudeLongitude, int coordinateSystemId = 4326)
        {
            //latitudeLongitude = Regex.Replace(latitudeLongitude, @"\s+", " ");
            var tokens = latitudeLongitude.Split(new[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 2)
            {
                throw new ArgumentException(nameof(latitudeLongitude));
            }

            var lat = double.Parse(tokens[0]);
            var lon = double.Parse(tokens[1]);
            return CreatePoint(lat, lon, coordinateSystemId);
        }

        public static bool TryCreatePoint(
            double latitude,
            double longitude,
            out DbGeography location,
            int coordinateSystemId = 4326)
        {
            try
            {
                location = CreatePoint(latitude, longitude, coordinateSystemId);
                return true;
            }
            catch
            {
                location = null;
                return false;
            }
        }

        public static bool TryCreatePoint(
            string latitudeLongitude,
            out DbGeography location,
            int coordinateSystemId = 4326)
        {
            try
            {
                location = CreatePoint(latitudeLongitude, coordinateSystemId);
                return true;
            }
            catch
            {
                location = null;
                return false;
            }
        }
    }
}