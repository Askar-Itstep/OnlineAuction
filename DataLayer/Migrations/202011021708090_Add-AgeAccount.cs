namespace OnlineAuction.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddAgeAccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "Age", c => c.Int(nullable: true));
        }

        public override void Down()
        {
            DropColumn("dbo.Accounts", "Age");
        }
    }
}
