namespace StackExchange.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CachingTotalCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderQueues", "TotalCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderQueues", "TotalCount");
        }
    }
}
