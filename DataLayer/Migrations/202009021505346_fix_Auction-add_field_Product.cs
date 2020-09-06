namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix_Auctionadd_field_Product : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "ProductId", c => c.Int(nullable: false));
            CreateIndex("dbo.Auctions", "ProductId");
            AddForeignKey("dbo.Auctions", "ProductId", "dbo.Products", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Auctions", "ProductId", "dbo.Products");
            DropIndex("dbo.Auctions", new[] { "ProductId" });
            DropColumn("dbo.Auctions", "ProductId");
        }
    }
}
