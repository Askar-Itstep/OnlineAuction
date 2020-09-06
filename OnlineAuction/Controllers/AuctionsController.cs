using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
        public ActionResult Index(int? isActor, string sms = null) //isActor - только из _Layout.html (опред. по клику "мои аукц.")
        {
            var accountId = Session["accountId"] ?? 0; //надо обязат. выйти
           
            //-----------------------------------------------------------
            //фишка: отправить на титул "Лучшее предложение" (пока самый дорогой, а надо по соотн. рыноч (стат.) и предлож. продавц. цены
            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            List<AuctionBO> auctionsBO = auctionBO.LoadAll().ToList();
            var prodBO = DependencyResolver.Current.GetService<ProductBO>();
            List<ProductBO> productsBO = prodBO.LoadAll().ToList();
            decimal maxPrice = productsBO.Max(p => p.Price);
            var bestAuctionBO = auctionsBO.Where(a => a.Product.Price >= maxPrice).FirstOrDefault();
            var bestAuctionVM = mapper.Map<AuctionVM>(bestAuctionBO);
            ViewBag.BestAuction = bestAuctionVM;
            //------могут быть 2 вида запроса: все аукционы; мои аукционы (актор), при этом попытка не-клиента должна пресек.---------
            ViewBag.Sms = "";
            if (sms != null && sms != "") {
                ViewBag.Sms = sms;
            }
            var auctionsVM = auctionsBO.Select(a => mapper.Map<AuctionVM>(a)).ToList();
            if (accountId != null && (int)accountId != 0 && isActor != null) {    //т.е. для "мои аукционы"
                auctionsVM = auctionsVM.Where(a => a.Actor.AccountId == (int)accountId).ToList();
            }
            return View(auctionsVM);
        }

        //1-передел. визуал
        //2- при клике по "сделать ставку" - для хозяина лота будет блокировка!!!!!
        public async Task<ActionResult> Details(int? auctionId)
        {
            if (auctionId == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(auctionId);
            if (auction == null) {
                return HttpNotFound();
            }
            return View(auction);
        }

        public ActionResult Create()  
        {
            var accountId = Session["accountId"] ?? 0;
            //сначала надо проверить явл. ли юзер moder'om?
            var roleAccountLinks = db.RoleAccountLinks.Where(r => r.AccountId == (int)accountId && r.Role.RoleName.Contains("moder")).ToList();
            if (roleAccountLinks == null || roleAccountLinks.Count() == 0) {
                return RedirectToAction("Index", new { sms = "Вы пока не можете создать лот. Проверте ваш баланс!" });
            }
            else {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string title, string description, decimal price, DateTime daybegin, TimeSpan timebegin,
                                                            float duration, HttpPostedFileBase upload)  {
            Auction auction = new Auction();
            if (ModelState.IsValid) //AuctionVM         
            {
                var userId = Session["accountId"]?? 0;
                if ((int)userId != 0) {
                    byte[] myBytes = new byte[upload.ContentLength];
                    upload.InputStream.Read(myBytes, 0, upload.ContentLength);
                    Image image = new Image { FileName = title, ImageData = myBytes };
                    db.Images.Add(image);

                    Product product = new Product { Image = image, Price = price, Title = title };
                    db.Products.Add(product);
                    var client = db.Clients.Where(c => c.AccountId == (int)userId).FirstOrDefault();

                    auction.ActorId = (int)client.Id;
                    auction.Product = product;
                    auction.BeginTime = daybegin + timebegin;
                    auction.EndTime = auction.BeginTime + TimeSpan.FromHours((int)duration);
                    auction.WinnerId = (int)client.Id; // в начале! - потом перезапишется
                    db.Auctions.Add(auction);
                   
                    BetAuction betAuction = new BetAuction { Auction = auction, Bet = price, Client = client };
                    db.BetAuction.Add(betAuction);

                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            return View(auction);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(id);
            if (auction == null) {
                return HttpNotFound();
            }
            //ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id", auction.ActorId);
            //ViewBag.ProductId = new SelectList(db.Products, "Id", "Id", auction.ProductId);
            //ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id", auction.WinnerId);
            return View(auction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ActorId,BeginTime,EndTime,ProductId,WinnerId")] Auction auction)
        {
            if (ModelState.IsValid) {
                db.Entry(auction).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id", auction.ActorId);
            //ViewBag.ProductId = new SelectList(db.Products, "Id", "Id", auction.ProductId);
            //ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id", auction.WinnerId);
            return View(auction);
        }

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
