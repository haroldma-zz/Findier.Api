namespace Findier.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAllowMessaging : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Posts", "AllowMessages");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "AllowMessages", c => c.Boolean(nullable: false));
        }
    }
}
