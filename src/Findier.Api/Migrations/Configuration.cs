using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using Findier.Api.Enums;
using Findier.Api.Extensions;
using Findier.Api.Models;
using Newtonsoft.Json;

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

        protected override void Seed(AppDbContext context)
        {
            return;
            var streamReader = File.OpenText($@"{AppDomain.CurrentDomain.BaseDirectory}..\Resources\categorias.json");
            var readToEnd = streamReader.ReadToEnd();
            var deserializeObject = JsonConvert.DeserializeObject<Test>(readToEnd);
            foreach (var test1 in deserializeObject.Data)
            {
                context.Categories.Add(new Category
                {
                    Title = test1.Title,
                    Description = test1.Text,
                    IsNsfw = test1.IsNsfw,
                    Country = Country.PR,
                    Slug = test1.Title.ToUrlSlug()
                });
            }
            context.SaveChanges();
        }

        public class Test
        {
            public List<Test1> Data { get; set; }

            public class Test1
            {
                public string Text { get; set; }

                public bool IsNsfw { get; set; }

                public string Title { get; set; }
            }
        }
    }
}