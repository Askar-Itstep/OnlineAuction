namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Table_AddressRef_Account : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Region = c.String(nullable: false, maxLength: 255),
                        City = c.String(nullable: false, maxLength: 255),
                        Street = c.String(nullable: false, maxLength: 255),
                        House = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Accounts", "AddressId", c => c.Int(nullable: true));
            CreateIndex("dbo.Accounts", "AddressId");
            AddForeignKey("dbo.Accounts", "AddressId", "dbo.Addresses", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Accounts", "AddressId", "dbo.Addresses");
            DropIndex("dbo.Accounts", new[] { "AddressId" });
            DropColumn("dbo.Accounts", "AddressId");
            DropTable("dbo.Addresses");
        }
    }
}
