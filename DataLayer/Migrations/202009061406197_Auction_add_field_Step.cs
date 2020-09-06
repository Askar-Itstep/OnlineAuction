namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Auction_add_field_Step : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "Step", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Auctions", "Step");
        }
    }
}
