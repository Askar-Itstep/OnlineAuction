namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Auctions_add_field_IsActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auctions", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Auctions", "IsActive");
        }
    }
}
