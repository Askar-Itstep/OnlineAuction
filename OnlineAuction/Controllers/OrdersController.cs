using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    public class OrdersController : Controller
    {
        private Model1 db = new Model1();
        private IMapper mapper;

        public OrdersController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public ActionResult Index(int? accountId)
        {
            //var orders = db.Orders.Include(o => o.Client);
            OrderBO orderBO = DependencyResolver.Current.GetService<OrderBO>();
            List<OrderBO> orders = orderBO.LoadAllWithInclude("Client").ToList();
            if (accountId != null) {
                orders = orders.Where(o => o.Client.AccountId == accountId).ToList();
            }
            List<OrderVM> ordersVM = orders.Select(o => mapper.Map<OrderVM>(o)).ToList();
            //2)
            //получить заказы с датами (только по связям с item, Product, Auction)
            var itemBO = DependencyResolver.Current.GetService<ItemBO>();
            IEnumerable<ItemBO> items = itemBO.LoadAll();

            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            IEnumerable<AuctionBO> auctions = auctionBO.LoadWithInclude("Product");
            var query = auctions.Join(items,
                a => a.Product.Id,
                i => i.Product.Id,
                (a, i) => new { a.Id, a.EndTime, a.Product,  i.Order });

            #region example query
            //foreach(var q in query) {
            //    System.Diagnostics.Debug.WriteLine($"FullName: {q.Order.Client.Account.FullName}");
            //    foreach(var item in q.Order.Items) {
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
                    os.Id,
                    os.Client,
                    os.IsApproved,
                    OrderAuctions = qs.Select(q=>new
                    {
                        AuctionId = q.Id, 
                        q.Product,
                        q.Order
                    })
                });

            #region example res
            //foreach (var r in res) {
            //    System.Diagnostics.Debug.WriteLine($"OrderID: {r.Id}");
            //    System.Diagnostics.Debug.WriteLine($"Client: {r.Client.Account.FullName}");
            //    foreach (var auct in r.OrderAuctions) {
            //        System.Diagnostics.Debug.WriteLine($"AuctionID: {auct.AuctionId}");
            //        foreach (var item in auct.Order.Items) {
            //            System.Diagnostics.Debug.WriteLine($"Product: {item.Product.Title}");
            //        }
            //    }
            //}
            #endregion

            //----------теперь из синтетика выгнать OrderBO и View.Bag_----------
            List<OrderBO> selectOrders = new List<OrderBO>();
            IEnumerable<OrderFullMapVM> sintetic = res.Select(r => new OrderFullMapVM
            {
                 OrderId = (int)r.Id, Client = mapper.Map<ClientVM>(r.Client), IsApproved = r.IsApproved,
                AuctionIds = r.OrderAuctions.Select(oa=>oa.AuctionId),   Products = r.OrderAuctions.Select(oa=>mapper.Map<ProductVM>(oa.Product))
            });
            return View(ordersVM);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null) {
                return HttpNotFound();
            }
            return View(order);
        }




        //после нажат. <Купить сейчас> или <положить в Корзину>
        public ActionResult Create(OrderVM orderVM, int? prodId, decimal? endPrice, int? auctionId, bool flagBuyNow)//ItemVM itemVM
        {
            //проверка на налич. прод.
            if (prodId == null) {
                return new JsonResult { Data = "Ошибка. Продукт не выбран!", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            //проверка: актор лота не может быть его клиентом!
            ClientBO clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO = clientBO.Load((int)orderVM.ClientId);
            AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            auctionBO = auctionBO.LoadAsNoTracking((int)auctionId);
            if (auctionBO.IsActive == false) {
                return new JsonResult { Data = new { message = "К сожалению, в этом аукционе уже есть победитель" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            if (clientBO.Id == auctionBO.ActorId) { //далее из Detales.html -> Auctions/Confirm(orderId=null)
                return new JsonResult { Data = new { message = "Ошибка. Вы не можете участвовать в аукционе!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            string message = "Данные записаны!";
            int orderId = 0;
            OrderBO orderBO = null;
            try {

                orderBO = (OrderBO)Session["orderBO"] ?? mapper.Map<OrderBO>(orderVM);
                orderBO.Items = null;
                //orderBO.Save(orderBO);
                OrderBO lastClientOrder = orderBO.LoadAll().Where(o => o.ClientId == clientBO.Id).Last();
                if (flagBuyNow == true) {
                    CloseAuction(endPrice, auctionId, auctionBO, orderBO);
                }
                else {
                    var prodBO = DependencyResolver.Current.GetService<ProductBO>();
                    prodBO = prodBO.Load((int)prodId);
                    endPrice = prodBO.Price;
                }
                orderId = AddToCart(prodId, endPrice, lastClientOrder);
                message = "Данные записаны!";
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                message = "Произошел сбой записи. Приносим извинения. Обратитесь позднее";
            }
            var data = new { message, orderId, flagBuyNow };
            Session["data"] = data;
            //->Detail.html/Ajax-> Confirm.html (оформл. бланка заказа с кнопкой оплатить) 
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private int AddToCart(int? prodId, decimal? endPrice, OrderBO lastOrder)
        {
            int orderId;
            var itemVM = new ItemVM { ProductId = (int)prodId, EndPrice = (int)endPrice };
            ItemBO itemMapBO = mapper.Map<ItemBO>(itemVM);
            itemMapBO.OrderId = lastOrder.Id;
            orderId = (int)lastOrder.Id;
            //itemMapBO.Save(itemMapBO);
            return orderId;
        }

        private static void CloseAuction(decimal? endPrice, int? auctionId, AuctionBO auctionBO, OrderBO orderBO)
        {
            auctionBO.EndTime = DateTime.Now;
            auctionBO.Winner = orderBO.Client;
            auctionBO.IsActive = false; //деактивир.
            //auctionBO.Save(auctionBO);

            //вн. изм. в BetAuction
            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            betAuctionBO.AuctionId = (int)auctionId;
            betAuctionBO.ClientId = (int)orderBO.ClientId;
            betAuctionBO.Bet = (decimal)endPrice;
            //betAuctionBO.Save(betAuctionBO);
        }

        public ActionResult Confirm(int? orderId)
        {
            if (orderId == null) {
                return RedirectToAction("Index", "Auctions");
            }
            if (Session["accountId"] == null) {
                return RedirectToAction("Index", "Auctions", new { alert = "Время сессии истекло выйдите и залогинтесь снова!" });
            }

            //проверка- можно смотреть только свой заказ (на случ. прямого перехода из адр. строки)
            OrderBO orderBO = DependencyResolver.Current.GetService<OrderBO>();
            orderBO = orderBO.LoadAllWithInclude("Items").FirstOrDefault(o => o.Id == orderId);
            ClientBO clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO = clientBO.Load((int)orderBO.ClientId);
            if ((int)Session["accountId"] != clientBO.AccountId) {
                return RedirectToAction("Index", "Auctions", new { alert = "У вас нет прав просмотра данной страницы!" });
            }
            //просмотр деталей заказа + <Оплатить>
            OrderVM orderVM = mapper.Map<OrderVM>(orderBO);
            //-------надо разделить случаи "Купить сразу" и "Ленивая Корзина"-------------------------------------
            var data = new { message = "", orderId = 0, flagBuyNow = false };
            data = Cast(data, Session["data"]);
            if (data.flagBuyNow == true) {

            }
            return View(orderVM);
        }
        private static T Cast<T>(T typeHolder, Object x)
        {
            return (T)x;
        }

        [HttpPost]      //+ IsApproved = true
        public ActionResult Confirm() //после нажат. <Оплатить>
        {
            OrderBO orderBO = (OrderBO)Session["orderBO"];
            orderBO.IsApproved = true;
            orderBO.Save(orderBO);
            return new JsonResult { Data = new { message = "Спасибо за покупки!", orderBO.Id } };
        }
        public ActionResult BuyBye(int? orderId)
        {
            //отправить SMS клиенту и в DHL?
            ViewBag.OrderId = orderId == null ? 0 : orderId;
            return View();
        }
        //[HttpPost]        //представление OrderCreate не нужно
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,ClientId,IsApproved")] Order order)
        //{
        //    if (ModelState.IsValid) {
        //        db.Orders.Add(order);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", order.ClientId);
        //    return View(order);
        //}


        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null) {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Order order = await db.Orders.FindAsync(id);
        //    if (order == null) {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", order.ClientId);
        //    return View(order);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,ClientId,IsApproved")] Order order)
        //{
        //    if (ModelState.IsValid) {
        //        db.Entry(order).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", order.ClientId);
        //    return View(order);
        //}

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null) {
                return HttpNotFound();
            }
            return View(order);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Order order = await db.Orders.FindAsync(id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
