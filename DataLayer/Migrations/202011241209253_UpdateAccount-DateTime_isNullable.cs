namespace OnlineAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAccountDateTime_isNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Accounts", "CreateAt", c => c.DateTime());
            AlterColumn("dbo.Accounts", "RemoveAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Accounts", "RemoveAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Accounts", "CreateAt", c => c.DateTime(nullable: false));
        }
    }
}
