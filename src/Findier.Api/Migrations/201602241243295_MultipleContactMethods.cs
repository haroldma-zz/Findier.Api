namespace Findier.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultipleContactMethods : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "CanMessage", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "Email", c => c.String());
            AddColumn("dbo.Posts", "PhoneNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "PhoneNumber");
            DropColumn("dbo.Posts", "Email");
            DropColumn("dbo.Posts", "CanMessage");
        }
    }
}
