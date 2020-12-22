using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using DataLayer.Repository;
using OnlineAuction.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
  
    public class BetAuctionsController : Controller 
    {
        private Model1 db = new Model1();
        private IMapper mapper;

        public BetAuctionsController(IMapper mapper)
        {
            this.mapper = mapper;
        }

       
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            var betAuctionsBO = DependencyResolver.Current.GetService<BetAuctionBO>().LoadAllWithInclude("Auction", "Client");
            var betAuctions = betAuctionsBO.Select(b => mapper.Map<BetAuction>(b));
            return View(betAuctions.Select(b => mapper.Map<BetAuctionVM>(b)));
        }

        [Authorize(Roles = "admin, moder")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            BetAuction betAuction = await betAuctionBO.FindByIdAsync(id);
            BetAuctionVM betAuctionVM = mapper.Map<BetAuctionVM>(betAuction);
            if (betAuction == null)
            {
                return HttpNotFound();
            }
            return View(betAuctionVM);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Create()
        {
            IEnumerable<AuctionBO> auctionsBO = DependencyResolver.Current.GetService<AuctionBO>().LoadAll();
            var auctions = auctionsBO.Select(a => mapper.Map<Auction>(a));
            ViewBag.AuctionId = new SelectList(auctions.Select(a => mapper.Map<AuctionVM>(a)), "Id", "Description");
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id");
            return View();
        }

        [Authorize(Roles = "admin, moder")]
        [HttpPost]
        public async Task<ActionResult> Create(BetAuctionVM betAuctionVM, JsonRequestCreateBet data)
        {
             BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
        //запрос из формы (использ. только автором аукциона при создании)
            if (ModelState.IsValid && betAuctionVM.AuctionId != 0
                ) 
            {
                BetAuction betAuction = mapper.Map<BetAuction>(betAuctionVM);
                BetAuctionBO betAuctionCreateBO = mapper.Map<BetAuctionBO>(betAuction);
                await betAuctionBO.SaveAsync(betAuctionCreateBO);
                return RedirectToAction("Index");
            }
            else
            {      //по запросу "Сделать ставку" в Auctions->Details.html
                string messageError = "";
                int auctionId = int.Parse(data.AuctionId);
                int clientId = int.Parse(data.ClientId);
                decimal bet = decimal.Parse(data.Bet);
                try
                {
                    var myBetAuctionVM = new BetAuctionVM { AuctionId = auctionId, ClientId = clientId, Bet = bet };
                    var myBetAuction = mapper.Map<BetAuction>(myBetAuctionVM);
                    //проверить на повторность ставки
                    var repeatBet = betAuctionBO.LoadAll().FirstOrDefault(b => b.AuctionId == auctionId && b.ClientId == clientId && b.Bet == bet);
                        //.FirstOrDefault(b => b.AuctionId == auctionId && b.ClientId == clientId && b.Bet == bet);
                    if (repeatBet != null)
                    {
                        messageError = "Такая ставка уже есть. Попробуйте снова!";
                        return new JsonResult { Data = messageError, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                    }

                    await betAuctionBO.SaveAsync(mapper.Map<BetAuctionBO>(myBetAuction));
                    messageError = "Ставка сделана. Данные добавлены!";
                }
                catch (Exception e)
                {
                    messageError = string.Format("Произошла ошибка: {0}. Данные не были добавлены!", e.Message);
                }
                return new JsonResult { Data = messageError, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
        }

        //не используется - в представлении изменения  только по SelectList (но можно вызвать из строки)
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuctionBO betAuctionBO = await DependencyResolver.Current.GetService<BetAuctionBO>().FindBOByIdAsync((int)id);   //.Load((int)id);
            BetAuction betAuction = mapper.Map<BetAuction>(betAuctionBO);
            if (betAuctionBO.Id == null || betAuctionBO.Id == 0)
            {
                return HttpNotFound();
            }
            BetAuctionVM betAuctionVM = mapper.Map<BetAuctionVM>(betAuction);
            ViewBag.AuctionId = new SelectList(db.Auctions, "Id", "Description", betAuctionVM.AuctionId);
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "Id", betAuctionVM.ClientId);
            return View(betAuctionVM);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,AuctionId,ClientId,Bet")] BetAuctionVM betAuction)
        {
            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            if (ModelState.IsValid)
            {
                BetAuction bet = mapper.Map<BetAuction>(betAuction);
                BetAuctionBO betBO = mapper.Map<BetAuctionBO>(bet);
                await betAuctionBO.SaveAsync(betBO);
                return RedirectToAction("Index");
            }
            IEnumerable<BetAuctionBO> betAuctions = betAuctionBO.LoadAll();
            IEnumerable<BetAuctionVM> betAuctionsVM = betAuctions.Select(b => mapper.Map<BetAuctionVM>(b));
            ViewBag.AuctionId = new SelectList(betAuctionsVM, "Id", "Description", betAuction.AuctionId);
            IEnumerable<ClientBO> clientsBO = DependencyResolver.Current.GetService<ClientBO>().LoadAll();
            var clients = clientsBO.Select(c => mapper.Map<Client>(c));
            var clientsVM= clients.Select(c => mapper.Map<ClientVM>(c));
            ViewBag.ClientId = new SelectList(clientsVM, "Id", "Id", betAuction.ClientId);
            return View(betAuction);
        }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BetAuctionBO betAuctionBO = await DependencyResolver.Current.GetService<BetAuctionBO>().FindBOByIdAsync((int)id);//.Load((int)id);

            if (betAuctionBO.Id == null || betAuctionBO.Id == 0)
            {
                return HttpNotFound();
            }
            BetAuction betAuction = mapper.Map<BetAuction>(betAuctionBO);
            BetAuctionVM betAuctionVM = mapper.Map<BetAuctionVM>(betAuction);
            return View(betAuctionVM);
        }

        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            BetAuctionBO betAuctionBO = await DependencyResolver.Current.GetService<BetAuctionBO>().FindBOByIdAsync(id);       //.Load((int)id);
            betAuctionBO.DeleteSave(betAuctionBO);
            return RedirectToAction("Index");
        }

    }
}
