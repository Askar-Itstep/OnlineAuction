namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixImageRemoveByte : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "URI", c => c.String());
            DropColumn("dbo.Images", "ImageData");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Images", "ImageData", c => c.Binary());
            DropColumn("dbo.Images", "URI");
        }
    }
}
