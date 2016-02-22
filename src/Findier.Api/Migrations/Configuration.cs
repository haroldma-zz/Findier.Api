using System.Data.Entity.Migrations;
using Findier.Api.Models;

namespace Findier.Api.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SqlClient", new AppSqlServerMigrationSqlGenerator());
        }
    }
}