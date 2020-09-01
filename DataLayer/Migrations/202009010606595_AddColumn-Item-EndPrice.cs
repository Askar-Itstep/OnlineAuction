namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnItemEndPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "EndPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "EndPrice");
        }
    }
}
