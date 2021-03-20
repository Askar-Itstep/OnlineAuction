using AutoMapper;
using BusinessLayer.BusinessObject;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    [Authorize(Roles = "admin, moder, client")]
    public class HomeController : Controller
    {
        private IMapper mapper;

        private static UserVM UserVM { get; set; }

        public HomeController(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public async Task<ActionResult> Index()
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId != 0)
            {
                AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                List<RoleAccountLinkBO> rolesAccount = DependencyResolver.Current.GetService<RoleAccountLinkBO>()
                                                                                .LoadAll().Where(r => r.AccountId == (int)accountId).ToList();
                //админ - не участв. в чате! (доп. контроль в представл. и контроллере Home/Chat()) 
                var roleAdmin = rolesAccount.FirstOrDefault(r => r.Role.RoleName.Contains("admin"));

                //поприветствовать при входе (видно только в чате)
                ChatHubService hubService = new ChatHubService(mapper: mapper);
                var tuple = await hubService.RunChatHubAsync(accountId, accountBO, roleAdmin);
                ViewBag.Message = tuple.Item1;  //может быть Good /Error
                UserVM = tuple.Item2;
                if (tuple.Item3)
                {
                    ViewBag.Firebase = true;
                }
            }
            
            return View();
        }

        //click <Чат> NavigationMenu
        [Authorize(Roles = "moder, client")]
        public ActionResult Chat()
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            ViewBag.User = UserVM.Account;
            return View("Partial/_ChatPartialView");
        }
        //sms - прилетают из формы Partial/_ChatPartialView 
        [Authorize(Roles = "moder, client")]
        public async Task<ActionResult> ChatAsync(string connectionId, string message, string friendConnectId)//1-ый парам. по 1-му заходу, 2-ой по 2-му
        {
            string alert = "";
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            else
            {
                ViewBag.User = null;
                var sender = PushSender.InstanceClient;
                if (UserVM.Account != null)
                {
                    ViewBag.User = UserVM.Account;
                }
                if (connectionId == "" || connectionId is null)
                {
                    return View("Partial/_ChatAuctionView");
                }
                ViewBag.Actor = null;
                message = message == "" ? "Hello!" : message;
                //addresser
                if (friendConnectId != null && friendConnectId != "")
                {
                    await sender.CommunicationWIthAuthorAsync(message, connectionId, friendConnectId);
                }
                else
                {
                    await sender.SendMessage(message, true); //+key SignIn
                }
                return new JsonResult { Data = alert, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}