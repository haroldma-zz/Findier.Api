namespace Findier.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CategoryTranslation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CategoryTranslations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        Language = c.Int(nullable: false),
                        Slug = c.String(nullable: false, maxLength: 45),
                        Title = c.String(nullable: false, maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            AddColumn("dbo.Categories", "Language", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CategoryTranslations", "CategoryId", "dbo.Categories");
            DropIndex("dbo.CategoryTranslations", new[] { "CategoryId" });
            DropColumn("dbo.Categories", "Language");
            DropTable("dbo.CategoryTranslations");
        }
    }
}
