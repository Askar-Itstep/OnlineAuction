namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixFieldNameRedemption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "RedemptionPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Auctions", "RedeptionPrice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Auctions", "RedeptionPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Auctions", "RedemptionPrice");
        }
    }
}
