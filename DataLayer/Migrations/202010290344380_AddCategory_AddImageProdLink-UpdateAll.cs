namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategory_AddImageProdLinkUpdateAll : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImageProductLinks",
                c => new
                    {
                        Id = c.Int(nullable: true, identity: true),
                        ProductId = c.Int(),
                        ImageId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Images", t => t.ImageId)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.ProductId)
                .Index(t => t.ImageId);

            CreateTable(
                "dbo.Categories",
                c => new
                {
                    Id = c.Int(nullable: true, identity: true),
                    Title = c.String(),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Accounts", "Gender", c => c.Int(nullable: true));
            AddColumn("dbo.Products", "CategoryId", c => c.Int());
            CreateIndex("dbo.Products", "CategoryId");
            AddForeignKey("dbo.Products", "CategoryId", "dbo.Categories", "Id");

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImageProductLinks", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ImageProductLinks", "ImageId", "dbo.Images");
            DropIndex("dbo.ImageProductLinks", new[] { "ImageId" });
            DropIndex("dbo.ImageProductLinks", new[] { "ProductId" });
            DropTable("dbo.ImageProductLinks");

            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropColumn("dbo.Products", "CategoryId");
            DropColumn("dbo.Accounts", "Gender");
            DropTable("dbo.Categories");
        }
    }
}
