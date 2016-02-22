using System.Data.Entity.Migrations;

namespace Findier.Api.Migrations
{
    public partial class CreatePost : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AppUsers", newName: "Users");
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        Text = c.String(),
                        UserId = c.Int(nullable: false),
                        EditedAt = c.DateTime(),
                        DeletedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FinboardId = c.Int(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        IsNsfw = c.Boolean(nullable: false),
                        Location = c.Geography(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Slug = c.String(nullable: false, maxLength: 45),
                        Text = c.String(),
                        Title = c.String(nullable: false, maxLength: 300),
                        Type = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        EditedAt = c.DateTime(),
                        DeletedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Finboards", t => t.FinboardId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.FinboardId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Finboards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Country = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        IsArchived = c.Boolean(nullable: false),
                        IsNsfw = c.Boolean(nullable: false),
                        Slug = c.String(nullable: false, maxLength: 45),
                        Title = c.String(nullable: false, maxLength: 100),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CommentVotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommentId = c.Int(nullable: false),
                        IpAddress = c.String(nullable: false),
                        IsUp = c.Boolean(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Comments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.CommentId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PostVotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IpAddress = c.String(nullable: false),
                        IsUp = c.Boolean(nullable: false),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentVotes", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.PostVotes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostVotes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "UserId", "dbo.Users");
            DropForeignKey("dbo.Finboards", "UserId", "dbo.Users");
            DropForeignKey("dbo.CommentVotes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "FinboardId", "dbo.Finboards");
            DropForeignKey("dbo.Comments", "PostId", "dbo.Posts");
            DropIndex("dbo.PostVotes", new[] { "UserId" });
            DropIndex("dbo.PostVotes", new[] { "PostId" });
            DropIndex("dbo.CommentVotes", new[] { "UserId" });
            DropIndex("dbo.CommentVotes", new[] { "CommentId" });
            DropIndex("dbo.Finboards", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "FinboardId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Comments", new[] { "PostId" });
            DropTable("dbo.PostVotes");
            DropTable("dbo.CommentVotes");
            DropTable("dbo.Finboards");
            DropTable("dbo.Posts");
            DropTable("dbo.Comments");
            RenameTable(name: "dbo.Users", newName: "AppUsers");
        }
    }
}
