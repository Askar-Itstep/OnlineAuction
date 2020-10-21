namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixAccountAddImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "ImageId", c => c.Int(nullable: true));
            CreateIndex("dbo.Accounts", "ImageId");
            AddForeignKey("dbo.Accounts", "ImageId", "dbo.Images", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Accounts", "ImageId", "dbo.Images");
            DropIndex("dbo.Accounts", new[] { "ImageId" });
            DropColumn("dbo.Accounts", "ImageId");
        }
    }
}
