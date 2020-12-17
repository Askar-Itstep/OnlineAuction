using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
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
        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "r7NFXF1rmprl3xQlJ8lmhdWyVguJqNprrnxnUA2P",
            BasePath = "https://asp-net-with-firebase-default-rtdb.firebaseio.com/"
        };
        private IFirebaseClient client;

        //isActor - только из _Layout.html (опред. по клику "мои аукц.")
        //CategoryId - параметр из формы для фильтр.
        public ActionResult Index(int? isActor, int? CategoryId, string alert = null)
        {
            ViewBag.Alert = "";
            if (alert != null && alert != "")
            {
                ViewBag.Alert = alert;  //alert - из методов Auctions/Create(), Orders/Confirm()..
            }
            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();

            //------могут быть 2 вида запроса: все аукционы; мои аукционы (актор)----------  
            var accountId = Session["accountId"] ?? 0; //re-sign!
            var isAdmin = Session["isAdmin"] ?? false;
            List<AuctionBO> auctionsBO = auctionBO.LoadAll().ToList();
            if (isAdmin != null)
            {
                auctionsBO = auctionsBO.Where(a => a.IsActive).ToList();
            }
            //var auctions = auctionsBO.Select(a => mapper.Map<Auction>(a)).ToList();
            var auctionsVM = auctionsBO.Select(a => mapper.Map<AuctionVM>(a));
            //т.е. для "мои аукционы"
            if (accountId != null && (int)accountId != 0 && isActor != null)
            {
                auctionsVM = auctionsVM.Where(a => a.Actor.AccountId == (int)accountId).ToList();
            }
            //--------на главн. разворот -------------------------
            ViewBag.BestAuction = 0;
            GetMainLot(auctionsBO, auctionsVM.ToList());

            //--------- --Categories-------------
            auctionsVM = GetCategories(CategoryId, auctionsVM.ToList());
            return View(auctionsVM);
        }

        private List<AuctionVM> GetCategories(int? CategoryId, List<AuctionVM> auctionsVM)
        {
            var categoriesBO = DependencyResolver.Current.GetService<CategoryBO>().LoadAll();
            var categories = categoriesBO.Select(c => mapper.Map<Category>(c));
            var categoriesVM = categories.Select(c => mapper.Map<CategoryVM>(c)).ToList();
            var emptyCategory = new CategoryVM { Id = 0, Title = "All" };
            categoriesVM.Add(emptyCategory);
            ViewBag.Categories = new SelectList(categoriesVM, "Id", "Title");
            if (CategoryId != null)
            {
                if (CategoryId != 0)
                {
                    auctionsVM = auctionsVM.Where(a => a.Product.CategoryId == CategoryId).ToList();
                }
            }
            return auctionsVM;
        }

        private void GetMainLot(List<AuctionBO> auctionsBO, List<AuctionVM> auctionsVM)
        {
            var prodBO = DependencyResolver.Current.GetService<ProductBO>();
            List<ProductBO> productsBO = prodBO.LoadAll().ToList();
            decimal maxPrice = productsBO.Max(p => p.Price);
            var bestAuctionBO = auctionsBO.Where(a => a.Product.Price >= maxPrice).FirstOrDefault();
            var bestAuction = mapper.Map<Auction>(bestAuctionBO);
            var bestAuctionVM = mapper.Map<AuctionVM>(bestAuction);
            ViewBag.BestAuction = bestAuctionVM;
            //------изъять глав аукц. из общего списка 
            var selectBest = auctionsVM.FirstOrDefault(a => a.Id == bestAuctionVM.Id);
            auctionsVM.Remove(selectBest);
        }

        //при клике  "сделать ставку" - для хозяина лота будет блокировка! (контроль в клиенте)
        public ActionResult Details(int? auctionId)
        {
            if (auctionId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Index", new { alert = "Время сессии истекло. Выйдите и зайдите снова!" });
            }
            var auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            auctionBO = auctionBO.Load((int)auctionId);
            if (auctionBO == null)
            {
                return HttpNotFound();
            }
            var betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            var topBetBO = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionBO.Id).ToList().Last();
            var topBet = mapper.Map<BetAuction>(topBetBO);
            var topBetVM = mapper.Map<BetAuctionVM>(topBet);
            ViewBag.TopBet = topBetVM;

            //all images product
            var imageProductLinkBO = DependencyResolver.Current.GetService<ImageProductLinkBO>();
            IEnumerable<ImageProductLinkBO> imageProductLinkBOs = imageProductLinkBO.LoadAll().Where(i => i.ProductId == auctionBO.ProductId);
            IEnumerable<ImageBO> imagesBO = imageProductLinkBOs.Select(i => i.Image);
            var images = imagesBO.Select(i => mapper.Map<Image>(i));
            ViewBag.ListImg = images.Select(i => mapper.Map<ImageVM>(i));


            //нужно для запроса в представл. Details.html  на созд. ставки, ajax->BetAuction/Create()
            //Client client = db.Clients.FirstOrDefault(c => c.AccountId == (int)accountId);
            ClientBO clientBO = DependencyResolver.Current.GetService<ClientBO>().LoadAll().FirstOrDefault(c => c.AccountId == (int)accountId);
            Client client = mapper.Map<Client>(clientBO);
            ViewBag.Client = mapper.Map<ClientVM>(client);
            var auction = mapper.Map<Auction>(auctionBO);
            return View(mapper.Map<AuctionVM>(auction));
        }

        //+Edit---------------------------------------------------------------------
        [Authorize(Roles = "member, admin")]
        public ActionResult Create(AuctionEditVM data, int? flagCreate, int? imageId) //ajax-метод Detai.html->click("Edit")
        {
            var request = Request.QueryString;
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Index", new { alert = "Вы сейчас не можете создать лот. Залогинтесь!" });
            }
            //сначала надо проверить явл. ли юзер moder'ом?
            var roleAccountLinks = db.RoleAccountLinks.Where(r => r.AccountId == (int)accountId && r.Role.RoleName.Contains("moder")).ToList();
            if (roleAccountLinks == null || roleAccountLinks.Count() == 0)
            {
                return new JsonResult { Data = new { success = false, alert = "You don't have editing permissions. Сheck your balance!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            //------------ Create -вернуть пуст. форму-----------------------
            if (flagCreate != null)
            {
                var categoriesBO = DependencyResolver.Current.GetService<CategoryBO>().LoadAll();
                var categories = categoriesBO.Select(c => mapper.Map<Category>(c));
                var categoriesVM = categories.Select(c => mapper.Map<CategoryVM>(c));
                ViewBag.Categories = new SelectList(categoriesVM, "Id", "Title");
                return View(new AuctionEditVM());
            }
            //---------------------------------------Edit--------------------------------------------------------------------------------
            else if (data != null && data.Id != 0)
            {
                var imageBO = DependencyResolver.Current.GetService<ImageBO>();
                ImageVM imageVM = null;
                if (imageId != null)
                {
                    imageBO = imageBO.Load((int)imageId);
                    imageVM = mapper.Map<ImageVM>(imageBO);
                }
                //подготов. данных для 2го захода->потом из ajax снова  в этот контроллер, в котор. вызвать объект из сессии
                Session["editImg"] = imageVM;
                Session["auctionEdit"] = data;
                return new JsonResult { Data = new { success = true, alert = "Форма редактирования подготовлена!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            //данные для 2-го захода (Edit)
            var editVM = Session["auctionEdit"];
            if (data.Id == 0 && editVM == null)
            {
                return new JsonResult { Data = new { success = false, alert = "Cбой передачи данных. Обратитесь к администратору!" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            ViewBag.editImg = Session["editImg"];
            ViewBag.Title = "Edit";
            return View(editVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "member, admin")]
        public async Task<ActionResult> Create(AuctionEditVM editVM, int? id, HttpPostedFileBase upload)
        {
            var userId = Session["accountId"] ?? 0;
            string message = "";
            //------------------------Create---------------------------------------
            if (id == 0)
            {
                AuctionBO auction = DependencyResolver.Current.GetService<AuctionBO>();
                if (ModelState.IsValid) //AuctionVM         
                {
                    if ((int)userId != 0)
                    {
                        BuilderSynteticModels.mapper = mapper;
                        auction = await BuilderSynteticModels.CreateEntity(editVM, auction, userId, upload);
                        message = "Данные записаны!";
                        //------установ. планировщика, триггер должен сработать по истеч. врмени аукциона
                        //------работа: выбор победителя, прекращ. аукциона, ставок, отправка писем
                        BetAuctionScheduler.DateEnd = auction.EndTime;
                        BetAuctionScheduler.AuctionId = auction.Id;
                        BetAuctionScheduler.mapper = mapper;
                        BetAuctionScheduler.Start();
                    }
                    else
                    {
                        message = "Ошибка. Данные не записаны. Время сессии истекло!";
                    }
                    return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                }
                return View(auction);
            }
            //------------------------Edit-------------------------------
            else if (id != null && id > 0)
            {
                AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
                auctionBO = auctionBO.LoadAsNoTracking((int)id);
                if (auctionBO == null)
                {
                    message = "Аукцион не найден. Попробуйте изменить запрос";
                    return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                }
                message = "Данные перезаписаны!";
                BuilderSynteticModels.mapper = mapper;
                await BuilderSynteticModels.EditEntityAsync(editVM, auctionBO, upload);
                return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
            else
            {
                message = "Неизвестная ошибка. Обратитесь позднее";
                return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }
        }

        #region OldCode Edit(int? id)
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
        #endregion
        [Authorize(Roles = "member, admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>().Load((int)id);
            if (auctionBO == null)
            {
                return HttpNotFound();
            }
            //var auction = mapper.Map<Auction>(auctionBO);
            return View(mapper.Map<AuctionVM>(auctionBO));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "member, admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>().Load((int)id);
            auctionBO.DeleteSave(auctionBO);
            return RedirectToAction("Index");
        }

        //=======================ChatHub============================
        //private static UserHubBO UserHub { get; set; }
        private static UserVM UserVM { get; set; }

        //1-ый заход: по клику ссылки "Связаться с Автором"
        //2-ой: по ajax-заходу (и активац. хаба) в _ChatAuctionView - для получ. ConnectID
        [Authorize(Roles = "member, client")]
        public async Task<ActionResult> ChatAsync(int? auctionId, string connectionId, string message)//1-ый парам. по 1-му заходу, 2-ой по 2-му
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            else
            {
                //----------------sender-------------------
                ViewBag.User = null;
                var sender = PushSender.InstanceClient;
                AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                //проверить если юзер-админ: выйти
                List<RoleAccountLinkBO> rolesAccount = DependencyResolver.Current.GetService<RoleAccountLinkBO>()
                                                                                                         .LoadAll().Where(r => r.AccountId == (int)accountId).ToList();
                var roleAdmin = rolesAccount.FirstOrDefault(r => r.Role.RoleName.Contains("admin"));

                ChatHubService hubService = new ChatHubService(mapper: mapper);
                var tuple = await hubService.RunChatHubAsync(accountId, accountBO, roleAdmin);
                UserVM = tuple.Item2;
                ViewBag.User = UserVM.Account;

                if (connectionId == "" || connectionId is null)
                {
                    return View("Partial/_ChatAuctionView");
                }
                //если connectId != "" (2-ой заход)
                //----------------addresser-------------------
                AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>().Load((int)auctionId);
                ViewBag.Actor = null;
                string alert = "";
                if (auctionBO != null)
                {
                    var actorBO = auctionBO.Actor;
                    var actorAccountBO = actorBO.Account;
                    var actorAccountVM = mapper.Map<AccountVM>(actorAccountBO);

                    ViewBag.Actor = actorAccountVM;
                    client = new FireSharp.FirebaseClient(config);
                    FirebaseResponse firebaseResponse = await client.GetAsync("Users");
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);

                    if (data != null)
                    {
                        List<UserVM> users = ChatHubService.GetUsersFirebase(data);
                        var actor = users.FirstOrDefault(u => u.AccountId == actorAccountVM.Id);
                        if (actor != null)
                        {
                            message = message.Equals("") ? "Hello author!" : message;
                            sender.mapper = mapper;
                            bool flag = await sender.CommunicationWIthAuthorAsync(message, connectionId, actor.ConnectionId);

                            PushSender.SaveMessage(message, accountId, actorBO);   //сохр. в БД
                            if (flag == true)
                            {
                                alert = "It's good";
                            }
                            else
                            {
                                alert = "Fail";
                            }
                        }
                    }
                    else
                    {
                        alert = "Actor is Null!";
                    }
                }
                return new JsonResult { Data = alert, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

    }
}
