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
    public class HomeController : Controller
    {
        //private Model1 db = new Model1();
        private IMapper mapper;
        private static UserHubBO UserHub { get; set; }

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
                var roleAdmin = rolesAccount.FirstOrDefault(r => r.Role.RoleName.Contains("admin"));
                if (accountBO != null && roleAdmin == null)
                {
                    UserHub = DependencyResolver.Current.GetService<UserHubBO>().LoadAll().Where(u => u.AccountId == (int)accountId).FirstOrDefault();
                    if (UserHub == null)
                    {
                        UserHubVM userHubVM = new UserHubVM { AccountId = (int)accountId, ConnectionId = "" };
                        UserHub = mapper.Map<UserHubBO>(userHubVM);
                        UserHub.Save(UserHub);
                    }
                    UserHub.Account = accountBO;
                    var sender = PushSender.InstanceClient;
                    sender.User = UserHub;
                    sender.mapper = mapper;
                    await sender.SendHello(string.Format("А у нас новый участник: {0}", UserHub.Account.FullName));
                }
            }
            return View();
        }
        public ActionResult Chat()
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            ViewBag.User = UserHub.Account;
            return View("Partial/_ChatPartialView");
        }
        public async Task<ActionResult> ChatAsync(string connectionId, string message, string friendConnectId)//1-ый парам. по 1-му заходу, 2-ой по 2-му
        {
            string alert = "";
            //sender
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            else
            {
                ViewBag.User = null;
                var sender = PushSender.InstanceClient;
                AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                if (accountBO != null)
                {
                    UserHub.Account = accountBO;
                    sender.User.Account = UserHub.Account;
                    ViewBag.User = UserHub.Account;
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
                    await sender.CommunicationWIthAuthor(message, connectionId, friendConnectId);
                }
                else
                {
                    await sender.SendMessage(message, true); //+key SignIn
                }
                return new JsonResult { Data = alert, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        public ActionResult SwitchContainer(string nameContainer)
        {
            if (nameContainer.ToUpper().Contains("AWS"))
            {

            }
            return RedirectToAction("Index");
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