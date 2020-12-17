using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    public class HomeController : Controller
    {
        private IMapper mapper;

        //using Firebase GoogleCloud-----------------------------
        private static UserVM UserVM { get; set; }
        //private IFirebaseConfig config = new FirebaseConfig
        //{
        //    AuthSecret = "r7NFXF1rmprl3xQlJ8lmhdWyVguJqNprrnxnUA2P",
        //    BasePath = "https://asp-net-with-firebase-default-rtdb.firebaseio.com/"
        //};
        //private IFirebaseClient client;


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
                ChatHubService hubService = new ChatHubService(mapper: mapper);
                var tuple  = await hubService.RunChatHubAsync(accountId, accountBO, roleAdmin);
                UserVM = tuple.Item2;
            }
            return View();
        }

        //private async Task RunChatHub(object accountId, AccountBO accountBO, RoleAccountLinkBO roleAdmin)
        //{
        //    if (accountBO != null && roleAdmin == null)
        //    {
        //        client = new FireSharp.FirebaseClient(config);
        //        FirebaseResponse firebaseResponse = await client.GetAsync("Users");
        //        dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);

        //        if (data != null)
        //        {
        //            var list = new List<UserVM>();
        //            foreach (var item in data)
        //            {
        //                //System.Diagnostics.Debug.WriteLine("dynamic: " + JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
        //                list.Add(JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
        //            }
        //            UserVM = list.Where(u => u.AccountId == (int)accountId).FirstOrDefault();
        //        }

        //          if (UserVM == null)//UserHubBO
        //        {
        //            //в табл. UserHubs - нет постоянн. данных! (при выходе из чата-данные удалятся)                    
        //            UserVM = new UserVM { Id="", AccountId = (int)accountId, ConnectionId = "" };
        //            //сохр. в GoogleFirebase
        //            try
        //            {
        //                InsertUserToFirebaseAsync(UserVM); //отступл. от правил (сохр. простой объект)
        //                ModelState.AddModelError(string.Empty, "Added Succefully");
        //            }
        //            catch (Exception e)
        //            {
        //                ModelState.AddModelError(string.Empty, e.Message);
        //            }
        //        }
        //        var account = mapper.Map<Account>(accountBO);

        //        //new model for Firebase 
        //        UserVM = new UserVM { Id="", AccountId = (int)accountId, ConnectionId = "" };
        //        UserVM.Account = mapper.Map<AccountVM>(account);

        //        //первый вызов инициализир. instance ChatHub
        //        var sender = PushSender.InstanceClient;
        //        sender.UserVM = UserVM;
        //        sender.mapper = mapper;
        //        await sender.SendHello(string.Format("А у нас новый участник: {0}", UserVM.Account.FullName));
        //    }
        //}

        //private  void InsertUserToFirebaseAsync(UserVM user)
        //{
        //    client = new FireSharp.FirebaseClient(config);
        //    PushResponse responce = client.Push("Users/", user);
        //    user.Id = responce.Result.name;
        //    SetResponse setResponse = client.Set("Users/" + user.Id, user);
        //}

        [Authorize(Roles = "member, client")]
        public ActionResult Chat()
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId == 0)
            {
                return RedirectToAction("Login", "Accounts");
            }
            ViewBag.User = UserVM.Account; //UserHubBO.Account;
            return View("Partial/_ChatPartialView");
        }
        //sms - прилетают из формы Partial/_ChatPartialView 
        [Authorize(Roles = "member, client")]
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
                if (UserVM.Account != null)  //accountBO
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

        //заглушка
        public ActionResult SwitchContainer(string platform)
        {
            //need create fabric connect to DB
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