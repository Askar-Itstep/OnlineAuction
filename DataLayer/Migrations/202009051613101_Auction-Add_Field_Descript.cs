namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionAdd_Field_Descript : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Auctions", "Description");
        }
    }
}
