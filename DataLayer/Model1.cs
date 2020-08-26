namespace OnlineAuction.Entities
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DataLayer.Entities;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        //public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<AuctionClientsLink> AuctionClientsLinks { get; set; }

        public virtual DbSet<BetAuction> BetAuction { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

    }

    public class MyContextInitializer : DropCreateDatabaseIfModelChanges<Model1>    //Always //
    {
        protected override void Seed(Model1 context)    //1-ое обращ. из AccauntC.\Login - ?
        {
            //1) Roles
            Role adminRole = new Role { RoleName = "admin" };
            Role memberRole = new Role { RoleName = "member" };
            Role clientRole = new Role { RoleName = "client" };

            context.Roles.Add(adminRole);
            context.Roles.Add(memberRole);
            context.Roles.Add(clientRole);

            ////2) Users
            //User userAdmin = new User
            //{
            //    FullName = 
            //    Email = 
            //    Password = 
            //};
            //context.Users.Add(userAdmin);

            ////3)Account
            Account account = new Account("admin","admin@mail.ru","admin");
            context.Account.Add(account);


            context.SaveChanges();
            base.Seed(context);
        }
    }
}
