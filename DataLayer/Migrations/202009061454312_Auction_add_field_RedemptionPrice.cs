namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Auction_add_field_RedemptionPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "RedeptionPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Auctions", "RedeptionPrice");
        }
    }
}
