namespace Findier.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
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
                        AllowMessages = c.Boolean(nullable: false),
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
                        EditedAt = c.DateTime(),
                        DeletedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Country = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        DisplayName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 254),
                        NormalizedEmail = c.String(nullable: false, maxLength: 254),
                        NormalizedUserName = c.String(nullable: false, maxLength: 20),
                        UserName = c.String(nullable: false, maxLength: 20),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true)
                .Index(t => t.NormalizedEmail, unique: true)
                .Index(t => t.NormalizedUserName, unique: true)
                .Index(t => t.UserName, unique: true);
            
            CreateTable(
                "dbo.AppUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
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
                "dbo.AppUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
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
            
            CreateTable(
                "dbo.AppUserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AppRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
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
                        IsPostUserDeleted = c.Boolean(nullable: false),
                        IsUserDeleted = c.Boolean(nullable: false),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AppRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AppUserRoles", "RoleId", "dbo.AppRoles");
            DropForeignKey("dbo.CommentVotes", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.PostVotes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostThreads", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostThreads", "UserId", "dbo.Users");
            DropForeignKey("dbo.ThreadMessages", "UserId", "dbo.Users");
            DropForeignKey("dbo.ThreadMessages", "ThreadId", "dbo.PostThreads");
            DropForeignKey("dbo.AppUserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostVotes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "UserId", "dbo.Users");
            DropForeignKey("dbo.AppUserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.Finboards", "UserId", "dbo.Users");
            DropForeignKey("dbo.CommentVotes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.AppUserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "FinboardId", "dbo.Finboards");
            DropForeignKey("dbo.Comments", "PostId", "dbo.Posts");
            DropIndex("dbo.AppRoles", "RoleNameIndex");
            DropIndex("dbo.PostThreads", new[] { "UserId" });
            DropIndex("dbo.PostThreads", new[] { "PostId" });
            DropIndex("dbo.ThreadMessages", new[] { "UserId" });
            DropIndex("dbo.ThreadMessages", new[] { "ThreadId" });
            DropIndex("dbo.AppUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AppUserRoles", new[] { "UserId" });
            DropIndex("dbo.PostVotes", new[] { "UserId" });
            DropIndex("dbo.PostVotes", new[] { "PostId" });
            DropIndex("dbo.AppUserLogins", new[] { "UserId" });
            DropIndex("dbo.CommentVotes", new[] { "UserId" });
            DropIndex("dbo.CommentVotes", new[] { "CommentId" });
            DropIndex("dbo.AppUserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "UserName" });
            DropIndex("dbo.Users", new[] { "NormalizedUserName" });
            DropIndex("dbo.Users", new[] { "NormalizedEmail" });
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.Finboards", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "FinboardId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Comments", new[] { "PostId" });
            DropTable("dbo.AppRoles");
            DropTable("dbo.PostThreads");
            DropTable("dbo.ThreadMessages");
            DropTable("dbo.AppUserRoles");
            DropTable("dbo.PostVotes");
            DropTable("dbo.AppUserLogins");
            DropTable("dbo.CommentVotes");
            DropTable("dbo.AppUserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.Finboards");
            DropTable("dbo.Posts");
            DropTable("dbo.Comments");
        }
    }
}
