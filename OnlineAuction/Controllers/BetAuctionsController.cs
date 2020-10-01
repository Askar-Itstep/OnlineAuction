using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    public class BetAuctionsController : Controller
    {
        private Model1 db = new Model1();

        // GET: BetAuctions
        public async Task<ActionResult> Index()
        {
            var betAuction = db.BetAuction.Include(b => b.Auction).Include(b => b.Client);
            return View(await betAuction.ToListAsync());
        }

        // GET: BetAuctions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuction betAuction = await db.BetAuction.FindAsync(id);
            if (betAuction == null) {
                return HttpNotFound();
            }
            return View(betAuction);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.AuctionId = new SelectList(db.Auctions, "Id", "Description");
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id");
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken] //не приним. JsonRequest??????
        public async Task<ActionResult> Create(BetAuction betAuction, JsonRequestCreateBet data)
        {
            if (ModelState.IsValid && betAuction.AuctionId != 0) {
                db.BetAuction.Add(betAuction);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else {      //возвр. Json в Details.html
                string messageError = "";
                int auctionId = int.Parse(data.AuctionId);
                int clientId = int.Parse(data.ClientId);
                decimal bet = decimal.Parse(data.Bet);
                try {
                    var myBetAuction = new BetAuction { AuctionId = auctionId, ClientId = clientId,  Bet = bet };
                    var repeatBet = await db.BetAuction.FirstOrDefaultAsync(b => b.AuctionId == auctionId && b.ClientId == clientId && b.Bet == bet);
                    if(repeatBet != null) {
                        messageError = "Такая ставка уже есть. Попробуйте снова!";
                        return new JsonResult { Data = messageError, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                    }
                    db.BetAuction.Add(myBetAuction);
                    await db.SaveChangesAsync();
                    messageError = "Ставка сделана. Данные добавлены!";
                }
                catch (Exception e) {
                    messageError = string.Format("Произошла ошибка: {0}. Данные не были добавлены!", e.Message);
                }
                return new JsonResult { Data = messageError, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
        }


        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuction betAuction = await db.BetAuction.FindAsync(id);
            if (betAuction == null) {
                return HttpNotFound();
            }
            ViewBag.AuctionId = new SelectList(db.Auctions, "Id", "Description", betAuction.AuctionId);
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", betAuction.ClientId);
            return View(betAuction);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,AuctionId,ClientId,Bet")] BetAuction betAuction)
        {
            if (ModelState.IsValid) {
                db.Entry(betAuction).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AuctionId = new SelectList(db.Auctions, "Id", "Description", betAuction.AuctionId);
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", betAuction.ClientId);
            return View(betAuction);
        }


        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuction betAuction = await db.BetAuction.FindAsync(id);
            if (betAuction == null) {
                return HttpNotFound();
            }
            return View(betAuction);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BetAuction betAuction = await db.BetAuction.FindAsync(id);
            db.BetAuction.Remove(betAuction);
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
