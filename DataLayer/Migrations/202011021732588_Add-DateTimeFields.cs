namespace OnlineAuction.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddDateTimeFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "CreateAt", c => c.DateTime(nullable: true));
            AddColumn("dbo.Accounts", "RemoveAt", c => c.DateTime(nullable: true));
        }

        public override void Down()
        {
            DropColumn("dbo.Accounts", "RemoveAt");
            DropColumn("dbo.Accounts", "CreateAt");
        }
    }
}
