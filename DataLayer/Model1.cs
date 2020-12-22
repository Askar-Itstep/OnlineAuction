namespace DataLayer.Entities
{
    using DataLayer.Entities;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.IO;
    using System.Web.Configuration;

    [DbConfigurationType(typeof(MyConfig))]
    public partial class Model1 : DbContext
    {
        //public Model1() : base("name=Model1")
        //{
        //}
        public Model1() : base(MyConfig.connectionString)
        {
        }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<ImageProductLink> ImageProductLinks { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<AuctionClientsLink> AuctionClientsLinks { get; set; }
        public virtual DbSet<BetAuction> BetAuction { get; set; }

        public virtual DbSet<RoleAccountLink> RoleAccountLinks { get; set; }

        //--------SignalR-----------------
        // MSSQL сейчас не исп-ся -> Google Firebase
        public virtual DbSet<UserHub> UserHubs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

}
