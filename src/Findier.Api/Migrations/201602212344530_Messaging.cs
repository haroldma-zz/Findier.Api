using System.Data.Entity.Migrations;

namespace Findier.Api.Migrations
{
    public partial class Messaging : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FromUserId = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        PostId = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        ToUserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.FromUserId)
                .ForeignKey("dbo.Users", t => t.ToUserId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.FromUserId)
                .Index(t => t.PostId)
                .Index(t => t.ToUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PostMessages", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostMessages", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.PostMessages", "FromUserId", "dbo.Users");
            DropIndex("dbo.PostMessages", new[] { "ToUserId" });
            DropIndex("dbo.PostMessages", new[] { "PostId" });
            DropIndex("dbo.PostMessages", new[] { "FromUserId" });
            DropTable("dbo.PostMessages");
        }
    }
}
