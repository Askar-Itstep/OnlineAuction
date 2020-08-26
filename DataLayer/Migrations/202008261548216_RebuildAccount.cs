namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RebuildAccount : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Accounts", "UserId", "dbo.Users");
            DropIndex("dbo.Accounts", new[] { "UserId" });
            AddColumn("dbo.Accounts", "FullName", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.Accounts", "Email", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.Accounts", "Password", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.Accounts", "UserId");
            DropTable("dbo.Users");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Password = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Accounts", "UserId", c => c.Int(nullable: false));
            DropColumn("dbo.Accounts", "Password");
            DropColumn("dbo.Accounts", "Email");
            DropColumn("dbo.Accounts", "FullName");
            CreateIndex("dbo.Accounts", "UserId");
            AddForeignKey("dbo.Accounts", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
