using System.Configuration;

namespace Findier.Api.Infrastructure
{
    public static class AppConfigurations
    {
        public static string HashidsSalt = ConfigurationManager.AppSettings["HashidsSalt"];
    }
}