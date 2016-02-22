using System.Data.Entity.Migrations;

namespace Findier.Api.Migrations
{
    public partial class Threads : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PostMessages", "FromUserId", "dbo.Users");
            DropForeignKey("dbo.PostMessages", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.PostMessages", "PostId", "dbo.Posts");
            DropIndex("dbo.PostMessages", new[] { "FromUserId" });
            DropIndex("dbo.PostMessages", new[] { "PostId" });
            DropIndex("dbo.PostMessages", new[] { "ToUserId" });
            CreateTable(
                "dbo.ThreadMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsRead = c.Boolean(nullable: false),
                        Text = c.String(nullable: false),
                        ThreadId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PostThreads", t => t.ThreadId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.ThreadId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PostThreads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DeletedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
            DropTable("dbo.PostMessages");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.PostThreads", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostThreads", "UserId", "dbo.Users");
            DropForeignKey("dbo.ThreadMessages", "UserId", "dbo.Users");
            DropForeignKey("dbo.ThreadMessages", "ThreadId", "dbo.PostThreads");
            DropIndex("dbo.PostThreads", new[] { "UserId" });
            DropIndex("dbo.PostThreads", new[] { "PostId" });
            DropIndex("dbo.ThreadMessages", new[] { "UserId" });
            DropIndex("dbo.ThreadMessages", new[] { "ThreadId" });
            DropTable("dbo.PostThreads");
            DropTable("dbo.ThreadMessages");
            CreateIndex("dbo.PostMessages", "ToUserId");
            CreateIndex("dbo.PostMessages", "PostId");
            CreateIndex("dbo.PostMessages", "FromUserId");
            AddForeignKey("dbo.PostMessages", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PostMessages", "ToUserId", "dbo.Users", "Id");
            AddForeignKey("dbo.PostMessages", "FromUserId", "dbo.Users", "Id");
        }
    }
}
