namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix_AuctionDel_Field_Order : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Auctions", "OrderId", "dbo.Orders");
            DropIndex("dbo.Auctions", new[] { "OrderId" });
            DropColumn("dbo.Auctions", "OrderId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Auctions", "OrderId", c => c.Int(nullable: false));
            CreateIndex("dbo.Auctions", "OrderId");
            AddForeignKey("dbo.Auctions", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
        }
    }
}
