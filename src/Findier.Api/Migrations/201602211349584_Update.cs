using System.Data.Entity.Migrations;

namespace Findier.Api.Migrations
{
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Finboards", "EditedAt", c => c.DateTime());
            AddColumn("dbo.Finboards", "DeletedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Finboards", "DeletedAt");
            DropColumn("dbo.Finboards", "EditedAt");
        }
    }
}
