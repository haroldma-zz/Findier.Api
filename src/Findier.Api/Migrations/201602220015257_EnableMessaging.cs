using System.Data.Entity.Migrations;

namespace Findier.Api.Migrations
{
    public partial class EnableMessaging : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "AllowMessages", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "AllowMessages");
        }
    }
}
