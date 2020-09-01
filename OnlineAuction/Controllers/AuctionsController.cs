using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLayer.Entities;
using OnlineAuction.Entities;

namespace OnlineAuction.Controllers
{
    public class AuctionsController : Controller
    {
        private Model1 db = new Model1();
        
        public async Task<ActionResult> Index(int? userId)
        {
            IQueryable<Auction> auctions = null;
            if (userId != null && userId != 0) {
                auctions = db.Auctions.Where(a=>a.ActorId == userId).Include(a => a.Actor).Include(a => a.Order).Include(a => a.Winner);
                //отправ. пакет для различ. состояния "клиент" и "актор"
                ViewBag.IsActor = 1;
            }
            //для админа
            else {
                auctions = db.Auctions.Include(a => a.Actor).Include(a => a.Order).Include(a => a.Winner);
            }                
            return View(await auctions.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }
        
        public ActionResult Create()
        {
            //ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id");
            //ViewBag.OrderId = new SelectList(db.Orders, "Id", "Id");
            //ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ActorId,BeginTime,EndTime,OrderId,WinnerId")] Auction auction)
        {
            if (ModelState.IsValid)
            {
                db.Auctions.Add(auction);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id", auction.ActorId);
            //ViewBag.OrderId = new SelectList(db.Orders, "Id", "Id", auction.OrderId);
            //ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id", auction.WinnerId);
            return View(auction);
        }
        
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id", auction.ActorId);
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Id", auction.OrderId);
            ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id", auction.WinnerId);
            return View(auction);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ActorId,BeginTime,EndTime,OrderId,WinnerId")] Auction auction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(auction).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ActorId = new SelectList(db.Clients, "Id", "Id", auction.ActorId);
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Id", auction.OrderId);
            ViewBag.WinnerId = new SelectList(db.Clients, "Id", "Id", auction.WinnerId);
            return View(auction);
        }
        
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = await db.Auctions.FindAsync(id);
            if (auction == null)
            {
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
