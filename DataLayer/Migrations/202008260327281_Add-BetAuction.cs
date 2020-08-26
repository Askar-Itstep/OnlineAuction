namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBetAuction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BetAuctions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuctionId = c.Int(nullable: false),
                        ClientId = c.Int(nullable: false),
                        Bet = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Auctions", t => t.AuctionId, cascadeDelete: true)
                .ForeignKey("dbo.Clients", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.AuctionId)
                .Index(t => t.ClientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BetAuctions", "ClientId", "dbo.Clients");
            DropForeignKey("dbo.BetAuctions", "AuctionId", "dbo.Auctions");
            DropIndex("dbo.BetAuctions", new[] { "ClientId" });
            DropIndex("dbo.BetAuctions", new[] { "AuctionId" });
            DropTable("dbo.BetAuctions");
        }
    }
} 
