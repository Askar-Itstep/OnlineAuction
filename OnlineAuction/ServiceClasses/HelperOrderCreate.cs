using AutoMapper;
using BusinessLayer.BusinessObject;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    public class HelperOrderCreate
    {
        private static Model1 db = new Model1();
        public static IMapper mapper;

        public HelperOrderCreate(IMapper mapper)
        {
        }
        //sql: select..auction join items on productId..join orders on itemId..group bu orderId
        public static IEnumerable<OrderFullMapVM> GetSynteticVM(List<OrderBO> orders)
        {
            var itemBO = DependencyResolver.Current.GetService<ItemBO>();
            IEnumerable<ItemBO> items = itemBO.LoadAll();

            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            IEnumerable<AuctionBO> auctions = auctionBO.LoadWithInclude("Product");
            var query = auctions.Join(items,
                a => a.Product.Id,
                i => i.Product.Id,
                (a, i) => new { a.Id, a.EndTime, a.Product, i.Order });

            #region example query
            //foreach (var q in query) {
            //    System.Diagnostics.Debug.WriteLine($"FullName: {q.Order.Client.Account.FullName}");
            //    foreach (var item in q.Order.Items) {
            //        System.Diagnostics.Debug.WriteLine($"Product: {item.Product.Title}");
            //    }
            //}
            #endregion

            var res = orders.GroupJoin(
                query,
                o => o.Id,
                q => q.Order.Id,
                (os, qs) => new
                {
                   Id = os.Id,
                   Client = os.Client,
                   IsApproved = os.IsApproved,
                    OrderAuctions = qs.Select(q => new
                    {
                        AuctionId = q.Id,
                        q.EndTime,
                        q.Product,
                        q.Order
                    })
                });

            #region example res
            ////System.Diagnostics.Debug.WriteLine($"Server:");
            ////foreach (var r in res)
            ////{
            ////    System.Diagnostics.Debug.WriteLine($"OrderID: {r.Id}");
            ////    System.Diagnostics.Debug.WriteLine($"Client: {r.Client.Account.FullName}");
            ////    foreach (var auct in r.OrderAuctions)
            ////    {
            ////        System.Diagnostics.Debug.WriteLine($"AuctionID: {auct.AuctionId}");
            ////        foreach (var item in auct.Order.Items)
            ////        {
            ////            System.Diagnostics.Debug.WriteLine($"Product: {item.Product.Title}");
            ////        }
            ////    }
            ////}
            #endregion

            //----------теперь получить синтетик - полн. карта списка заказов  ----------
            IEnumerable<OrderFullMapVM> syntetic = res.Select(r => new OrderFullMapVM
            {
                Id = (int)r.Id,
                Client = mapper.Map<ClientVM>(r.Client),
                IsApproved = r.IsApproved,
                AuctionIds = r.OrderAuctions.Select(oa => oa.AuctionId).ToList(),//.ToArray(),
                EndTimes = r.OrderAuctions.Select(oa => oa.EndTime),
                Products = r.OrderAuctions.Select(oa => mapper.Map<ProductVM>(oa.Product))
            });

            #region Syntetic.Products isEmpty?
            //IEnumerable<OrderFullMapVM> syntetic2 = new List<OrderFullMapVM>();
            //foreach (var order in res)
            //{
            //    var orderMap = new OrderFullMapVM();
            //    orderMap.Id = (int)order.Id;
            //    orderMap.Client = mapper.Map<ClientVM>(order.Client);
            //    orderMap.IsApproved = order.IsApproved;
            //    orderMap.AuctionIds = new List<int>();
            //    orderMap.EndTimes = new List<DateTime>();
            //    orderMap.Products = new List<ProductVM>();
            //    foreach (var auct in order.OrderAuctions)
            //    {
            //        orderMap.AuctionIds.Add(auct.AuctionId);
            //        orderMap.EndTimes.ToList().Add(auct.EndTime);
            //        orderMap.Products.ToList().Add(mapper.Map<ProductVM>(auct.Product));
            //    }
            //}
            #endregion

            return syntetic;
        }

        public static int AddToCart(int? prodId, decimal? endPrice, OrderBO lastOrder)
        {
            int orderId;
            var itemVM = new ItemVM { ProductId = (int)prodId, EndPrice = (int)endPrice };
            ItemBO itemMapBO = mapper.Map<ItemBO>(itemVM);
            itemMapBO.OrderId = lastOrder.Id;
            orderId = (int)lastOrder.Id;
            itemMapBO.Save(itemMapBO);
            return orderId;
        }

        public static BetAuctionBO CloseAuction(AuctionBO auctionBO, decimal endPrice = 0, OrderBO orderBO = null, List<BetAuctionBO> bets = null)   //int? auctionId,
        {
            auctionBO.EndTime = DateTime.Now;
            auctionBO.Winner = orderBO.Client;  //только при созд. аукц.
            auctionBO.IsActive = false; //деактивир.
            BetAuctionBO topBetAuction = null;
            if (bets == null) {//вн. изм. в BetAuction (для <Купить> и <Положить в корз>)                
                BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
                betAuctionBO.AuctionId = auctionBO.Id;
                betAuctionBO.ClientId = (int)orderBO.ClientId;
                betAuctionBO.Bet = endPrice;
                betAuctionBO.Save(betAuctionBO);
            }
            else { //выюор победителя по заверш. аукц.
                decimal topBet = bets.Max(b => b.Bet);
                topBetAuction = bets.FirstOrDefault(b => b.Bet == topBet);
                ClientBO winner = topBetAuction.Client;
                auctionBO.Winner = winner;
            }
            auctionBO.Save(auctionBO);
            return topBetAuction;
        }
        //для получ. аноним. объекта из сессии в OrderController/Confirm()
        public static T Cast<T>(T typeHolder, Object x)
        {
            return (T)x;
        }

        public static void GetOrderWithClient(int? orderId, out OrderBO orderBO, out ClientBO clientBO)
        {
            orderBO = DependencyResolver.Current.GetService<OrderBO>();
            orderBO = orderBO.LoadAllWithInclude("Items").FirstOrDefault(o => o.Id == orderId);
            clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO = clientBO.Load((int)orderBO.ClientId);
        }

        public static void GetClientWithAuction(OrderVM orderVM, int? auctionId, out ClientBO clientBO, out AuctionBO auctionBO)
        {
            clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO = clientBO.Load((int)orderVM.ClientId);
            auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            auctionBO = auctionBO.LoadAsNoTracking((int)auctionId);
        }




    }
}