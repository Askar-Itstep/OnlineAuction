using DataLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repository
{
    interface IUnitOfWork
    {
        BaseRepository<Account> Accounts { get; }
        BaseRepository<Auction> Auctions { get; }
        BaseRepository<AuctionClientsLink> AuctionClientsLinks { get; }
        BaseRepository<BetAuction> BetAuctions { get; }
        BaseRepository<Client> Clients { get; }
        BaseRepository<Image> Images { get; }
        BaseRepository<Item> Items { get; }
        BaseRepository<Message> Messages { get; }
        BaseRepository<Order> Orders { get; }
        BaseRepository<Product> Products { get; }
        BaseRepository<Role> Roles { get; }
        BaseRepository<RoleAccountLink> RoleAccountLinks { get; }
        BaseRepository<UserHub> UserHubs { get; }
        BaseRepository<ImageProductLink> ImageProductLinks { get; }
        BaseRepository<Category> Categories { get; }
    }
}
