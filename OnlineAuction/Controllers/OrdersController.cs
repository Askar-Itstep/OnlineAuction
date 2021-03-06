﻿using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Schedulers;
using OnlineAuction.ServiceClasses;
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
            HelperOrderCreate.mapper = mapper;
        }

        [Authorize(Roles = "admin, client")]
        public ActionResult Index(int? accountId, string alert)
        {
            OrderBO orderBO = DependencyResolver.Current.GetService<OrderBO>();
            List<OrderBO> ordersBO = orderBO.LoadAllWithInclude("Client").ToList();
            if (accountId != null)
            {
                ordersBO = ordersBO.Where(o => o.Client.AccountId == accountId).ToList();
            }
            List<Order> orders = ordersBO.Select(o => mapper.Map<Order>(o)).ToList();
            //2)
            //получить заказы с датами (только по связям с item, Product, Auction)
            //HelperOrderCreate.mapper = mapper;
            IEnumerable<OrderFullMapVM> syntetic = HelperOrderCreate.GetSynteticVM(orders.FindAll(o => o.IsApproved)); //оплач.
            IEnumerable<OrderFullMapVM> synteticBad = HelperOrderCreate.GetSynteticVM(orders.FindAll(o => !o.IsApproved));  //не оплач. заказы
            ViewBag.BadOrders = synteticBad;
            return View(syntetic);
        }

        [HttpPost]
        //кнопка Index.html->click Details заблокир. (не нужна)-но метод исп. для редактир.!
        //+Edit
        public ActionResult Details(OrderFullMapVM orderFullMap, int? flagDetail)
        {
            if (flagDetail is null)
            {                   //для Edit
                if (orderFullMap == null)
                {
                    return new JsonResult { Data = new { success = false, message = "Error. Object is Null!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                OrderVM order = mapper.Map<OrderVM>(orderFullMap);
                if (order == null)
                {
                    return new JsonResult { Data = new { success = false, message = "Error. Order is Null!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                Session["orderFullMap"] = orderFullMap;
                return new JsonResult { Data = new { success = true, message = "It's good!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                if (orderFullMap is null || orderFullMap.Id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                return View(orderFullMap);
            }
        }

        [Authorize(Roles = "client")]
        //Detals.html ->  <Купить сейчас> или <положить в Корзину>------
        public ActionResult Create(OrderVM orderVM, int? prodId, decimal endPrice, int? auctionId, bool flagBuyNow)
        {
            //проверка на налич. прод.
            if (prodId == null)
            {
                return new JsonResult { Data = "Ошибка. Продукт не выбран!", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            //проверка: актор лота не может быть его клиентом!
            HelperOrderCreate.GetClientWithAuction(orderVM, auctionId, out ClientBO clientBO, out AuctionBO auctionBO);
            if (auctionBO.IsActive == false)
            {
                return new JsonResult { Data = new { message = "К сожалению, в этом аукционе уже есть победитель" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            if (clientBO.Id == auctionBO.ActorId)
            { //оформл. заказа -> Auctions/Confirm(orderId=null)
                return new JsonResult { Data = new { message = "Ошибка. Вы не можете участвовать в аукционе!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            string message = "";
            int orderId = 0;
            OrderBO lastClientOrder = null;
            ItemBO itemBO = null;
            int itemsCount = 0;
            try
            {                    //------в корзину в любом случ. (цена выкуп =redemptionPrice)---------
                (lastClientOrder, itemBO) = HelperOrderCreate.AddToCart(prodId, endPrice, orderVM); //запись items
                lastClientOrder = GetLastOrder(clientBO,  lastClientOrder, itemBO);

                message = "Данные записаны!";
                itemsCount = lastClientOrder.Items.Count;
                orderId = (int)lastClientOrder.Id;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                message = "Произошел сбой записи. Приносим извинения. Обратитесь позднее";
            }
            var data = new { message, orderId, itemsCount, flagBuyNow, auctionId };
            Session["data"] = data;

            //если flagBuyNow-true: Detail.html/Ajax-> Confirm.html (оформл. бланка заказа с кнопкой оплатить) 
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private OrderBO GetLastOrder( ClientBO clientBO, OrderBO orderBO, ItemBO itemBO)
        {
            orderBO.ClientId = clientBO.Id;
            orderBO.Save(orderBO);

            itemBO.OrderId = orderBO.Id;
            itemBO.Save(itemBO);
            var newOrderBO = DependencyResolver.Current.GetService<OrderBO>().LoadAll().LastOrDefault();
            return newOrderBO;
        }

        [Authorize(Roles = "client")]
        //------------------after call back Create->ajax---------------------------------
        public ActionResult Confirm(int? orderId)
        {
            if (orderId == null)
            {
                return RedirectToAction("Index", "Auctions");
            }
            if (Session["accountId"] == null)
            {
                return RedirectToAction("Index", "Auctions", new { alert = "Время сессии истекло выйдите и залогинтесь снова!" });
            }
            //проверка- можно смотреть только свой заказ (на случ. прямого перехода из адр. строки)
            //HelperOrderCreate.mapper = mapper;
            HelperOrderCreate.GetOrderWithClient(orderId, out OrderBO orderBO, out ClientBO clientBO);
            if ((int)Session["accountId"] != clientBO.AccountId)
            {
                return RedirectToAction("Index", "Auctions", new { alert = "У вас нет прав просмотра данной страницы!" });
            }
            Session["orderBO"] = orderBO;
            //просмотр деталей заказа + <Оплатить>
            OrderVM orderVM = mapper.Map<OrderVM>(orderBO);
            return View(orderVM);
        }



        //прилож не учит. взаимодейств. с юзером после оконч. аукциона  - опред. победит. по ставкам (треб. оплаты, формы оплаты..)
        [HttpPost]
        public async Task<ActionResult> Confirm() //после нажат. <Оплатить>
        {
            OrderBO orderBO = null;
            if (Session["orderBO"] == null)
            {
                return new JsonResult { Data = new { success = false, message = "Error. Session is null!" } };
            }
            orderBO = (OrderBO)Session["orderBO"];
            orderBO.IsApproved = true;
            orderBO.Save(orderBO);
            dynamic sess = Session["data"];
            var data = sess.auctionId;
            int auctionId = (int)data;

            //-------------закрыть аукцион! (выкуп по макс цене >=redemptionPrice)-----------------------------------------
            var auctionBO = HelperOrderCreate.CloseAuction(auctionId, orderBO);
            //------------отправить письма участникам о заверш. аукц.-----------------
            EmailScheduler.AuctionId = auctionBO.Id;
            EmailScheduler.WinnerId = auctionBO.WinnerId;
            EmailScheduler.Start();

            var orderId = orderBO == null ? 0 : orderBO.Id;
            return new JsonResult { Data = new { message = "Спасибо за покупки!", orderId } };

        }

        public ActionResult BuyBye(int? orderId)
        {
            ViewBag.OrderId = orderId == null ? 0 : orderId;
            return View();
        }

        //===========================================================
        //2-ой заход после Details (Index.html->ajax->Details)
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int? id)
        {
            var orderFullMap = (OrderFullMapVM)Session["orderFullMap"];
            if (orderFullMap == null)
            {
                return new JsonResult { Data = new { success = false, message = "Error. Object is Null!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            if (orderFullMap.Id == 0)
            {
                new JsonResult { Data = new { success = false, message = "Error. OrderId Not Found!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return View(orderFullMap);
        }


        [HttpPost]
        public async Task<ActionResult> EditAsync(OrderFullMapVM orderFullMap)
        {
            if (ModelState.IsValid)
            {
                OrderBO orderBO = DependencyResolver.Current.GetService<OrderBO>().LoadAsNoTracking(orderFullMap.Id);
                BuilderSynteticModels.mapper = mapper;
                await BuilderSynteticModels.EditEntityAsync(orderFullMap, orderBO);

                return new JsonResult { Data = new { success = true, message = "Данные перезаписаны!" }, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
            return new JsonResult { Data = new { success = false, message = "Извините. Что-то пошло не так!(" }, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
