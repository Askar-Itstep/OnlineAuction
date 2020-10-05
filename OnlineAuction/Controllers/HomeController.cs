using AutoMapper;
using BusinessLayer.BusinessObject;
using OnlineAuction.Entities;
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
        private Model1 db = new Model1();
        private IMapper mapper;
        private static AccountVM Account { get; set; }

        public HomeController(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public async Task<ActionResult> Index()
        {
            var accountId = Session["accountId"] ?? 0;
            if ((int)accountId != 0) {
                AccountBO account = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                List<RoleAccountLinkBO> rolesAccount= DependencyResolver.Current.GetService<RoleAccountLinkBO>()
                    .LoadAll().Where(r=>r.AccountId==(int)accountId).ToList();
                var roleAdmin = rolesAccount.FirstOrDefault(r => r.Role.RoleName.Contains("admin"));
                if (account != null && roleAdmin == null) {
                    Account = mapper.Map<AccountVM>(account);
                    var sender = PushSender.Instance;
                    sender.Account = Account;
                    await sender.SendMessage(string.Format("А у нас новый участник: {0}", Account.FullName));
                }
            }
            return View();
        }
        public ActionResult Chat()
        {
                var accountId = Session["accountId"] ?? 0;
                if ((int)accountId == 0) {
                    return RedirectToAction("Login", "Accounts");
                }
                ViewBag.User = Account;
                return View("Partial/_ChatPartialView");  
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