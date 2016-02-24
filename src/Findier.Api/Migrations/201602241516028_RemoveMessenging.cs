namespace Findier.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMessenging : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ThreadMessages", "ThreadId", "dbo.PostThreads");
            DropForeignKey("dbo.ThreadMessages", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostThreads", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostThreads", "PostId", "dbo.Posts");
            DropIndex("dbo.ThreadMessages", new[] { "ThreadId" });
            DropIndex("dbo.ThreadMessages", new[] { "UserId" });
            DropIndex("dbo.PostThreads", new[] { "PostId" });
            DropIndex("dbo.PostThreads", new[] { "UserId" });
            DropColumn("dbo.Posts", "CanMessage");
            DropTable("dbo.ThreadMessages");
            DropTable("dbo.PostThreads");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PostThreads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPostUserDeleted = c.Boolean(nullable: false),
                        IsUserDeleted = c.Boolean(nullable: false),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Posts", "CanMessage", c => c.Boolean(nullable: false));
            CreateIndex("dbo.PostThreads", "UserId");
            CreateIndex("dbo.PostThreads", "PostId");
            CreateIndex("dbo.ThreadMessages", "UserId");
            CreateIndex("dbo.ThreadMessages", "ThreadId");
            AddForeignKey("dbo.PostThreads", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PostThreads", "UserId", "dbo.Users", "Id");
            AddForeignKey("dbo.ThreadMessages", "UserId", "dbo.Users", "Id");
            AddForeignKey("dbo.ThreadMessages", "ThreadId", "dbo.PostThreads", "Id", cascadeDelete: true);
        }
    }
}
