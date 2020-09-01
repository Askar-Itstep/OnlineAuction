namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoleAccountLink : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoleAccountLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoleAccountLinks", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.RoleAccountLinks", "AccountId", "dbo.Accounts");
            DropIndex("dbo.RoleAccountLinks", new[] { "AccountId" });
            DropIndex("dbo.RoleAccountLinks", new[] { "RoleId" });
            DropTable("dbo.RoleAccountLinks");
        }
    }
}
