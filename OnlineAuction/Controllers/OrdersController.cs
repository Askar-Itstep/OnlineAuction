using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
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
        }

        public ActionResult Index(int? accountId, string alert)
        {
            OrderBO orderBO = DependencyResolver.Current.GetService<OrderBO>();
            List<OrderBO> orders = orderBO.LoadAllWithInclude("Client").ToList();
            if (accountId != null)
            {
                orders = orders.Where(o => o.Client.AccountId == accountId).ToList();
            }
            List<OrderVM> ordersVM = orders.Select(o => mapper.Map<OrderVM>(o)).ToList();
            //2)
            //получить заказы с датами (только по связям с item, Product, Auction)
            HelperOrderCreate.mapper = mapper;
            IEnumerable<OrderFullMapVM> syntetic = HelperOrderCreate.GetSynteticVM(orders);

            return View(syntetic);
        }

        //кнопка Index.html->click Details заблокир. (не нужна)-но метод исп. для редактир.!
        //+Edit
        public ActionResult Details(OrderFullMapVM orderFullMap, int? flagDetail)
        {
            if (flagDetail is null)
            { //для Edit
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

        //--------Detals.html ->  <Купить сейчас> или <положить в Корзину>------
        public ActionResult Create(OrderVM orderVM, int? prodId, decimal endPrice, int? auctionId, bool flagBuyNow)//ItemVM itemVM
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
            string message = "Данные записаны!";
            int orderId = 0;
            //OrderBO orderBO = null;
            try
            {
                GetLastClientOrder(orderVM, clientBO,  out OrderBO lastClientOrder);
                if (!flagBuyNow) //------в корзину (возможн. выкуп =redemptionPrice)
                {
                    var prodBO = DependencyResolver.Current.GetService<ProductBO>();
                    prodBO = prodBO.Load((int)prodId);
                    //endPrice = prodBO.Price; //себестоимость?
                }
                HelperOrderCreate.mapper = mapper;
                orderId = HelperOrderCreate.AddToCart(prodId, endPrice, lastClientOrder); //запись items
                message = "Данные записаны!";

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                message = "Произошел сбой записи. Приносим извинения. Обратитесь позднее";
            }
            var data = new { message, orderId, flagBuyNow,  auctionId};
            Session["data"] = data;
            // flagBuyNow(true)->Detail.html/Ajax-> Confirm.html (оформл. бланка заказа с кнопкой оплатить) 
            //в Layout - сделать кнопку корзины с датчиком!!!!!!!
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private void GetLastClientOrder(OrderVM orderVM, ClientBO clientBO,  out OrderBO lastClientOrder)
        {
            var orderSession = (OrderBO)Session["orderBO"];
            var orderBO = orderSession ?? mapper.Map<OrderBO>(orderVM);
            orderBO.Items = null;
            orderBO.Save(orderBO);
            lastClientOrder = orderBO.LoadAll().Where(o => o.ClientId == clientBO.Id).Last();
        }

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
            HelperOrderCreate.GetOrderWithClient(orderId, out OrderBO orderBO, out ClientBO clientBO);

            if ((int)Session["accountId"] != clientBO.AccountId)
            {
                return RedirectToAction("Index", "Auctions", new { alert = "У вас нет прав просмотра данной страницы!" });
            }          
            Session["orderBO"] = orderBO;
            OrderVM orderVM = mapper.Map<OrderVM>(orderBO);
            //просмотр деталей заказа + <Оплатить>
            return View(orderVM);
        }


        //прилож не учит. взаимодейств. с юзером после оконч. аукциона  - опред. победит. по ставкам (треб. оплаты, формы оплаты..)
        [HttpPost]
        public async Task<ActionResult> Confirm() //после нажат. <Оплатить>
        {
            OrderBO orderBO = null;
            if (Session["orderBO"] != null)
            {
                orderBO = (OrderBO)Session["orderBO"];
                orderBO.IsApproved = true;
                orderBO.Save(orderBO);
            }
            dynamic sess = Session["data"];
            var r = sess.auctionId;
            int auctionId = (int)r;

            //-------------закрыть аукцион! (выкуп по макс цене >=redemptionPrice)-----------------------------------------
            var auctionBO= HelperOrderCreate.CloseAuction(auctionId, orderBO);
      //------------отправить письма участникам о заверш. аукц.-----------------
            EmailScheduler.AuctionId = auctionBO.Id;
            EmailScheduler.WinnerId = auctionBO.WinnerId;
            EmailScheduler.Start();

            var bundle = orderBO == null ? 0 : orderBO.Id;
            return new JsonResult { Data = new { message = "Спасибо за покупки!", bundle } };
        }

            public ActionResult BuyBye(int? orderId)
        {

            ViewBag.OrderId = orderId == null ? 0 : orderId;
            return View();
        }

        //=========================================================================
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
