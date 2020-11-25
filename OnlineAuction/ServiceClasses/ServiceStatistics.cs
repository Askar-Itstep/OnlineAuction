using AutoMapper;
using BusinessLayer.BusinessObject;
using OnlineAuction.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    public class ServiceStatistics
    {
        public static IMapper mapper;
        public static IEnumerable<StatisticViewModel> CreateStatisticModel()
        {
            //IEnumerable<ClientBO> clients = DependencyResolver.Current.GetService<ClientBO>().LoadAllWithInclude("Account");
            IEnumerable<ProductBO> productsBO = DependencyResolver.Current.GetService<ProductBO>().LoadAllWithInclude("Category");
            var products = productsBO.Select(p => mapper.Map<ProductVM>(p));

            IEnumerable<AuctionBO> auctionsBO = DependencyResolver.Current.GetService<AuctionBO>().LoadAllWithInclude("BetAuction");
            var auctions = auctionsBO.Select(p => mapper.Map<AuctionVM>(p));

            IEnumerable<ItemBO> itemsBO = DependencyResolver.Current.GetService<ItemBO>().LoadAllWithInclude("Product");
            var items = itemsBO.Select(p => mapper.Map<ItemVM>(p));

            IEnumerable<OrderBO> ordersBO = DependencyResolver.Current.GetService<OrderBO>().LoadAllWithInclude("Client");
            var orders = ordersBO.Select(p => mapper.Map<OrderVM>(p));

            //1)внутр. декарт. соедин.
            var modelPartZero = orders.SelectMany(o => o.Items,
            (o, i) => new //StatisticViewModel
            {
                OrderId = o.Id,
                IsApproved = o.IsApproved,
                AccountId = o.Client.AccountId,
                Account = o.Client.Account,
                ProductId = i.ProductId,
                Product = i.Product
            }).ToList();
            //2) left outer join
            var modelPartOne = from leftItem in auctions
                               join rightItem in modelPartZero
                               on leftItem.ProductId equals rightItem.ProductId
                               into grouping
                               from subRightItem in grouping.DefaultIfEmpty()
                               select new
                               {
                                   OrderId = (subRightItem == null ? 0 : subRightItem.OrderId),
                                   IsApproved = (subRightItem == null ? false : subRightItem.IsApproved),
                                   AuctionId = leftItem.Id,
                                   ProductId = leftItem.ProductId,
                                   Product = leftItem.Product
                               };
            #region Print
            //foreach (var item in modelPartOne)
            //{
            //    System.Diagnostics.Debug.WriteLine("auctID: {0}, prodID: {1} isApproved{2}, orderID: {3}", item.AuctionId, item.ProductId, item.IsApproved, item.OrderId);
            //}
            #endregion

            //3)все аукционы, вкл. с неоформл. заказами, +sum, max
            List<BetAuctionBO> betAuctionsBO = DependencyResolver.Current.GetService<BetAuctionBO>().LoadAll().ToList();
            var betAuctions = betAuctionsBO.Select(b => mapper.Map<BetAuctionVM>(b));
            var modelPartTwo = from row in betAuctions
                               group row by new
                               {
                                   row.Auction,
                                   row.Client.AccountId,
                                   row.Client.Account,
                                   row.Bet
                               } into grouping
                               let count = grouping.Count()
                               let maxBet = grouping.Max(g => g.Bet)
                               select new StatisticViewModel
                               {
                                   AuctionId = grouping.Key.Auction.Id,
                                   DateOrder = grouping.Key.Auction.EndTime,
                                   AccountId = grouping.Key.AccountId,
                                   Account = grouping.Key.Account,
                                   ProductId = grouping.Key.Auction.ProductId,
                                   Product = grouping.Key.Auction.Product,
                                   CountBet = count,
                                   MaxBet = maxBet //макс.ставка игрока по лоту
                               };
            modelPartTwo = modelPartTwo.OrderBy(g => g.AuctionId);
            #region Print
            //foreach (var item in modelPartTwo)
            //{
            //    System.Diagnostics.Debug.WriteLine("AuctionId:{0}, AccountID:   {1}, ProductId: {2}, Count:{3}, MaxBet: {4}",
            //         item.AuctionId, item.AccountId, item.ProductId, item.CountBet, item.MaxBet);
            //}
            #endregion
            //3) 2-3 LEFT OUTER JOIN
            return from leftItem in modelPartTwo
                   join rightItem in modelPartOne
                   on leftItem.AuctionId equals rightItem.AuctionId
                   into grouping
                   from subRightItem in grouping.DefaultIfEmpty()
                   select new StatisticViewModel
                   {
                       AuctionId = leftItem.AuctionId,
                       AccountId = leftItem.AccountId,
                       DateOrder = leftItem.DateOrder,
                       Account = leftItem.Account,
                       ProductId = leftItem.ProductId,
                       Product = leftItem.Product,
                       CountBet = leftItem.CountBet,
                       MaxBet = leftItem.MaxBet,
                       IsBuy = (subRightItem.IsApproved) // == null ? false : true)
                   };
        }
    }
}