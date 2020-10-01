using DataLayer.Entities;
using OnlineAuction.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repository
{
    //Паттерн Unit of Work позволяет упростить работу с различными репозиториями - 
    //    т.е. все репозитории будут использовать один и тот же контекст данных.
    public class UnitOfWork : IUnitOfWork
    {
        private BaseRepository<Account> accounts;
        private BaseRepository<Auction> auctions;
        private BaseRepository<Address> addresses;
        private BaseRepository<AuctionClientsLink> auctionClientsLinks;
        private BaseRepository<BetAuction> betAuctions;
        private BaseRepository<Client> clients;
        private BaseRepository<Image> images;
        private BaseRepository<Item> items;
        private BaseRepository<Message> messages;
        private BaseRepository<Order> orders;
        private BaseRepository<Product> products;
        private BaseRepository<Role> roles;
        private BaseRepository<RoleAccountLink> roleAccountLinks;
        private Model1 db;
        

        public UnitOfWork()
        {
            db = new Model1();
        }
        public BaseRepository<Account> Accounts
        {
            get
            {
                if (accounts == null)
                    accounts = new BaseRepository<Account>();
                return accounts;
            }
        }

        public BaseRepository<Auction> Auctions
        {
            get
            {
                if (auctions == null)
                    auctions = new BaseRepository<Auction>();
                return auctions;
            }
        }
        public BaseRepository<Address> Addresses
        {
            get
            {
                if (addresses == null)
                    addresses = new BaseRepository<Address>();
                return addresses;
            }
        }
        public BaseRepository<AuctionClientsLink> AuctionClientsLinks
        {
            get
            {
                if (auctionClientsLinks == null)
                    auctionClientsLinks = new BaseRepository<AuctionClientsLink>();
                return auctionClientsLinks;
            }
        }


        public BaseRepository<BetAuction> BetAuctions
        {
            get
            {
                if (betAuctions == null)
                    betAuctions = new BaseRepository<BetAuction > ();
                return betAuctions;
            }
        }

        public BaseRepository<Client> Clients
        {
            get
            {
                if (clients == null)
                    clients = new BaseRepository<Client>();
                return clients;
            }
        }

        public BaseRepository<Image> Images
        {
            get
            {
                if (images == null)
                    images = new BaseRepository<Image>();
                return images;
            }
        }

        public BaseRepository<Item> Items
        {
            get
            {
                if (items == null)
                    items = new BaseRepository<Item>();
                return items;
            }
        }

        public BaseRepository<Message> Messages
        {
            get
            {
                if (messages == null)
                    messages = new BaseRepository<Message>();
                return messages;
            }
        }

        public BaseRepository<Order> Orders
        {
            get
            {
                if (orders == null)
                    orders = new BaseRepository<Order>();
                return orders;
            }
        }

        public BaseRepository<Product> Products
        {
            get
            {
                if (products == null)
                    products = new BaseRepository<Product>();
                return products;
            }
        }
        public BaseRepository<Role> Roles
        {
            get
            {
                if (roles == null)
                    roles = new BaseRepository<Role>();
                return roles;
            }
        }

        public BaseRepository<RoleAccountLink> RoleAccountLinks {
            get
            {
                if (roleAccountLinks == null)
                    roleAccountLinks = new BaseRepository<RoleAccountLink>();
                return roleAccountLinks;
            }
        }

    }
}
