namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserHub : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserHubs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ConnectionId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserHubs", "AccountId", "dbo.Accounts");
            DropIndex("dbo.UserHubs", new[] { "AccountId" });
            DropTable("dbo.UserHubs");
        }
    }
}
