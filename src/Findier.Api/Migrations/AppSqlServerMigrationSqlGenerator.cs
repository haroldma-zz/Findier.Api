using System;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection;
using Findier.Api.Attributes;

namespace Findier.Api.Migrations
{
    internal class AppSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        internal Type GetTypeByName(string name)
        {
            name = name.Split('.').Last().Replace("ries", "ry");

            if (name.StartsWith("__"))
            {
                return null;
            }

            // not the fastest way but this only runs when executing a migration so not really a problem
            var assembly = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(x => name.Contains(x.Name)
                    && name.Length - x.Name.Length <= 2);
            return assembly;
        }

        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            ProccessColumn(addColumnOperation.Column, GetTypeByName(addColumnOperation.Table));

            base.Generate(addColumnOperation);
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            foreach (var columnModel in createTableOperation.Columns)
            {
                ProccessColumn(columnModel, GetTypeByName(createTableOperation.Name));
            }

            base.Generate(createTableOperation);
        }

        private static void ProccessColumn(PropertyModel column, Type tableType)
        {
            var columnProperty = tableType?.GetProperties().FirstOrDefault(p => p.Name == column.Name);
            var defaultValueAttribute = columnProperty?.GetCustomAttribute<EfDefaultValueAttribute>();

            if (defaultValueAttribute == null)
            {
                return;
            }
            if (defaultValueAttribute.Sql != null)
            {
                column.DefaultValueSql = defaultValueAttribute.Sql;
            }
            else
            {
                column.DefaultValue = defaultValueAttribute.Value;
            }
        }
    }
}