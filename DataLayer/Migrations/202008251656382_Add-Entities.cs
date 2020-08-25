namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuctionClientsLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuctionId = c.Int(nullable: false),
                        ClientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Auctions", t => t.AuctionId, cascadeDelete: true)
                .ForeignKey("dbo.Clients", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.AuctionId)
                .Index(t => t.ClientId);
            
            CreateTable(
                "dbo.Auctions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActorId = c.Int(nullable: false),
                        BeginTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        OrderId = c.Int(nullable: false),
                        WinnerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ActorId, cascadeDelete: false)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Clients", t => t.WinnerId, cascadeDelete: false)
                .Index(t => t.ActorId)
                .Index(t => t.OrderId)
                .Index(t => t.WinnerId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        PartnerId = c.Int(nullable: false),
                        Sms = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("dbo.Clients", t => t.PartnerId, cascadeDelete: false)
                .Index(t => t.ClientId)
                .Index(t => t.PartnerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "PartnerId", "dbo.Clients");
            DropForeignKey("dbo.Messages", "ClientId", "dbo.Clients");
            DropForeignKey("dbo.AuctionClientsLinks", "ClientId", "dbo.Clients");
            DropForeignKey("dbo.AuctionClientsLinks", "AuctionId", "dbo.Auctions");
            DropForeignKey("dbo.Auctions", "WinnerId", "dbo.Clients");
            DropForeignKey("dbo.Auctions", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Auctions", "ActorId", "dbo.Clients");
            DropIndex("dbo.Messages", new[] { "PartnerId" });
            DropIndex("dbo.Messages", new[] { "ClientId" });
            DropIndex("dbo.Auctions", new[] { "WinnerId" });
            DropIndex("dbo.Auctions", new[] { "OrderId" });
            DropIndex("dbo.Auctions", new[] { "ActorId" });
            DropIndex("dbo.AuctionClientsLinks", new[] { "ClientId" });
            DropIndex("dbo.AuctionClientsLinks", new[] { "AuctionId" });
            DropTable("dbo.Messages");
            DropTable("dbo.Auctions");
            DropTable("dbo.AuctionClientsLinks");
        }
    }
}
