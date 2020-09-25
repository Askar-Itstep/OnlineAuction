using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.Schedulers;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    public class AuctionsController : Controller
    {
        private Model1 db = new Model1();
        private IMapper mapper;
        public AuctionsController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        //параметр sms - из метода Create (проверка на роль)
        public ActionResult Index(int? isActor, string alert = null) //isActor - только из _Layout.html (опред. по клику "мои аукц.")
        {                                                               //alert - из методов Auctions/Create, Orders/Confirm
            var accountId = Session["accountId"] ?? 0; //надо обязат. выйти

            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            List<AuctionBO> auctionsBO = auctionBO.LoadAll().Where(a => a.IsActive).ToList();
            var prodBO = DependencyResolver.Current.GetService<ProductBO>();
            List<ProductBO> productsBO = prodBO.LoadAll().ToList();
            decimal maxPrice = productsBO.Max(p => p.Price);

            //на главн. разворот (~газеты)-------------------------
            var bestAuctionBO = auctionsBO.Where(a => a.Product.Price >= maxPrice).FirstOrDefault();
            var bestAuctionVM = mapper.Map<AuctionVM>(bestAuctionBO);
            ViewBag.BestAuction = bestAuctionVM;

            //------могут быть 2 вида запроса: все аукционы; мои аукционы (актор), при этом попытка не-клиента должна пресек.---------
            ViewBag.Alert = "";
            if (alert != null && alert != "") {
                ViewBag.Alert = alert;
            }
            var auctionsVM = auctionsBO.Select(a => mapper.Map<AuctionVM>(a)).ToList();
            if (accountId != null && (int)accountId != 0 && isActor != null) {    //т.е. для "мои аукционы"
                auctionsVM = auctionsVM.Where(a => a.Actor.AccountId == (int)accountId).ToList();
            }
            return View(auctionsVM);
        }

        //1-передел. визуал
        //2- при клике по "сделать ставку" - для хозяина лота будет блокировка! (контроль в клиенте)
        public async Task<ActionResult> Details(int? auctionId)
        {
            if (auctionId == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(auctionId);
            if (auction == null) {
                return HttpNotFound();
            }
            BetAuction topBet = db.BetAuction.Where(b => b.AuctionId == auction.Id).ToList().Last();
            ViewBag.TopBet = topBet;

            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0) {
                return RedirectToAction("Index", new { alert = "Время сессии истекло. Выйдите и зайдите снова!" });
            }
            Client client = db.Clients.FirstOrDefault(c => c.AccountId == (int)accountId);
            ViewBag.Client = client; //нужно для запроса с представл. Details.html  на созд. ставки -> BetAuction/Create()
            return View(auction);
        }

        //+Edit
        public ActionResult Create(AuctionEditVM data, int? flagCreate, int? imageId) //data, imageId - возвр. JSON ajax-метод Detai.html->click("Edit")
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0) {  //Create
                return RedirectToAction("Index", new { alert = "Вы сейчас не можете создать лот. Залогинтесь!" });
            }
            //сначала надо проверить явл. ли юзер moder'om?
            var roleAccountLinks = db.RoleAccountLinks.Where(r => r.AccountId == (int)accountId && r.Role.RoleName.Contains("moder")).ToList();
            if (roleAccountLinks == null || roleAccountLinks.Count() == 0) {
                return RedirectToAction("Index", new { alert = "Вы пока не можете создать лот. Проверте ваш баланс!" });
            }
            if (flagCreate != null) {
                return View(new AuctionEditVM());
            }
            else if (data != null && data.Id != 0) {      //Edit
                var imageBO = DependencyResolver.Current.GetService<ImageBO>();
                ImageVM imageVM = null;
                if (imageId != null) {
                    ImageBO image = imageBO.Load((int)imageId);
                    imageVM = mapper.Map<ImageVM>(image);
                }
                //подготов. данных для 2го захода->потом из ajax снова  в этот контроллер, в котор. вызвать объект из сессии
                Session["editImg"] = imageVM;
                Session["auctionEdit"] = data;
                return new JsonResult { Data = "Форма редактирования подготовлена!", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            //данные для 2-го захода (Edit)
            var editVM = Session["auctionEdit"];
            ViewBag.editImg = Session["editImg"];
            ViewBag.Title = "Edit";
            return View(editVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AuctionEditVM editVM, int? id, HttpPostedFileBase upload)//
        {
            var userId = Session["accountId"] ?? 0;
            string message = "";
            if (id == 0) {   //Create
                //Auction auction = new Auction();
                AuctionBO auction = DependencyResolver.Current.GetService<AuctionBO>();
                if (ModelState.IsValid) //AuctionVM         
                {
                    if ((int)userId != 0) {
                        BuilderSynteticModels.mapper = mapper;
                        auction = await BuilderSynteticModels.CreateEntity(editVM, auction, userId, upload);
                        message = "Данные записаны!";
                        //планировщик должен сработать при истечении врмени аукциона
                        //а)триггер-время окончания аукциона
                        //б)вызыв. работа-выбор победителя, прекращ. аукциона, ставок, отправка писем
                        BetAuctionScheduler.DateBegin = auction.BeginTime;
                        BetAuctionScheduler.DateEnd = auction.EndTime;
                        BetAuctionScheduler.Start();
                    }
                    else {
                        message = "Ошибка. Данные не записаны!";
                    }
                    return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                }
                return View(auction);
            }
            else if (id == null) {      //Edit
                AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
                auctionBO = auctionBO.LoadAsNoTracking((int)id);
                if (auctionBO == null) {
                    //return HttpNotFound();
                    message = "Неизвестн. Обратитесь позднее";
                    return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                }
                message = "Данные перезаписаны!";
                BuilderSynteticModels.mapper = mapper;
                await BuilderSynteticModels.EditEntityAsync(editVM, auctionBO, upload);
                return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
            else {
                message = "Неизвестная ошибка. Обратитесь позднее";
                return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }

        }


        //[Authorize(Roles = "moder")]
        //public async Task<ActionResult> Edit(int? id) //по ID-редактир. админ, а модер должен имет возм ред-ть продукт,
        //{                                                       //но не ред-ть организ. лота и победителя
        //    if (id == null) {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Auction auction = await db.Auctions.FindAsync(id);
        //    if (auction == null) {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ActorId = new SelectList(db.Clients.Include(c => c.Account), "Id", "Account.FullName", auction.ActorId);
        //    ViewBag.ProductId = new SelectList(db.Products, "Id", "Title", auction.ProductId);
        //    ViewBag.WinnerId = new SelectList(db.Clients.Include(c => c.Account), "Id", "Account.FullName", auction.WinnerId);
        //    return View(auction);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]

        //public async Task<ActionResult> Edit(
        ////[Bind(Include = "Id,ActorId,BeginTime,EndTime,ProductId,WinnerId")]
        //Auction auction)
        //{
        //    if (ModelState.IsValid) {
        //        db.Entry(auction).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ActorId = new SelectList(db.Clients.Include(c => c.Account), "Id", "Account.FullName", auction.ActorId);
        //    ViewBag.ProductId = new SelectList(db.Products, "Id", "Title", auction.ProductId);
        //    ViewBag.WinnerId = new SelectList(db.Clients.Include(c => c.Account), "Id", "Account.FullName", auction.WinnerId);
        //    return View(auction);
        //}

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(id);
            if (auction == null) {
                return HttpNotFound();
            }
            return View(auction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Auction auction = await db.Auctions.FindAsync(id);
            db.Auctions.Remove(auction);
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
