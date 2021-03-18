using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace OnlineAuction.Controllers
{
    public class AccountsController : Controller
    {
        private Model1 db = new Model1();
        private IMapper mapper;
        public AccountsController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [Authorize(Roles = "admin")]    //кнопка "Пользователи" (админ-панель)
        public ActionResult Index()
        {
            AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>();
            IEnumerable<AccountBO> accountBOs = accountBO.LoadAll().Where(a => a.IsActive);
            IEnumerable<AccountVM> accountVMs = accountBOs.Select(a => mapper.Map<AccountVM>(a));

            IEnumerable<AccountBO> delAccountBOs = accountBO.LoadAll().Where(a => !a.IsActive);
            IEnumerable<AccountVM> delAccountVMs = delAccountBOs.Select(a => mapper.Map<AccountVM>(a));
            ViewBag.DelAccounts = delAccountVMs;
            return View(accountVMs);
        }


        [Authorize(Roles = "admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().Load((int)id);
            AccountVM account = mapper.Map<AccountVM>(accountBO);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }


        //контроль роли  в представл.-_Layout
        //id - для админа, accountId - для user'a
        //flag - ключ из _Layout.html для выбора: редакт. акк. или пополн. баланс
        public async Task<ActionResult> Edit(int? id, int? flag)
        {
            //----------для юзера---------------
            if (id == null)
            {
                var accountId = Session["accountId"] ?? 0;

                if ((int)accountId == 0)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                AccountBO account = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                if (account == null)
                {
                    return HttpNotFound("Пользователь не найден");
                }
                if (flag == null)
                {
                    return View(mapper.Map<AccountVM>(account));
                }
                else
                {
                    return new JsonResult { Data = account, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            //---------для админа--------------
            else
            {
                AccountBO account = DependencyResolver.Current.GetService<AccountBO>().Load((int)id);
                if (account == null)
                {
                    return HttpNotFound("Пользователь не найден");
                }
                return View(mapper.Map<AccountVM>(account));
            }
        }

        [HttpPost]      //+ addBalance!
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AccountVM account, decimal? balance)
        {
            if (account.Id != null)
            { //при простом изм. аккаунта
                if (ModelState.IsValid)
                {
                    account.Address.Id = account.AddressId;
                    AddressVM address = account.Address;
                    AccountBO accountBO = mapper.Map<AccountBO>(account);
                    AddressBO addressBO = mapper.Map<AddressBO>(address);
                    accountBO.Save(accountBO);  //address -> cascade?
                    addressBO.Save(addressBO);
                    return View(account);
                }
            }
            else if (balance != null)
            { //при изм. баланса - добав. роль <Moder>(созд. лота)
                var accountId = (int)Session["accountId"];
                AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().LoadAllNoTracking().FirstOrDefault(a => a.Id == accountId);
                accountBO.AddBalance((decimal)balance);
                accountBO.Save(accountBO);
                //добав. роль "модер" -если такой нет
                var roleBO = DependencyResolver.Current.GetService<RoleBO>();
                var moderRole = roleBO.LoadAll().Where(r => r.RoleName.Contains("moder")).FirstOrDefault();
                if (moderRole == null)
                {
                    roleBO = IsRole(roleBO, "moder");
                    moderRole = roleBO.LoadAll().Where(r => r.RoleName.Contains("moder")).FirstOrDefault();
                }
                RoleAccountLinkVM linkVM = new RoleAccountLinkVM { AccountId = (int)accountId, RoleId = moderRole.Id };
                RoleAccountLinkBO linkBO = mapper.Map<RoleAccountLinkBO>(linkVM);
                linkBO.Save(linkBO);
            }
            return RedirectToAction("Index", "Home");
        }


        //контроль роли в представл.
        //удал. производ. административно - по письму к админу
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(id);
            if (account == null)
            {
                return HttpNotFound("Пользователь не найден");
            }
            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Account account = await db.Account.FindAsync(id);
            //db.Account.Remove(account); //нужно не удалять, а помечать как удаленный!
            account.IsActive = false;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var accountBO = DependencyResolver.Current.GetService<AccountBO>();
                var roleAccountBO = DependencyResolver.Current.GetService<RoleAccountLinkBO>();

                var accountBOList = accountBO.LoadAll().Where(a => a.IsActive); //только действующие аккаунты
                accountBO = accountBOList.FirstOrDefault(u => (u.Email.Contains(model.Login)
                                                                || u.Email.Contains(model.Login)) && u.Password.Equals(model.Password));
                if (accountBO == null || accountBO.Id == null || accountBO.Id == 0)
                {
                    //return RedirectToAction("Index", "Home");
                    return Json(new { success = false, message = "Account not found!" });
                }
                else
                {
                    var roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                    List<RoleBO> rolesBO = roleAccountBOList.Where(r => r.AccountId == accountBO.Id).Select(r => r.Role).ToList();
                    accountBO.RolesBO = rolesBO;

                    //связь котор. надо удалить или добавить-нет денег удал. роль клиента
                    RoleAccountLinkBO linkRoleClientAccount = roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault();    //client
                    if (accountBO.Balance <= 0)
                    {

                        if (linkRoleClientAccount != null)
                        {
                            roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault().DeleteSave(linkRoleClientAccount);
                        }
                    }
                    else
                    {  //..если есть деньги, но роль "модер" удалена или не было
                        if (linkRoleClientAccount == null)
                        {
                            RoleBO roleBO = DependencyResolver.Current.GetService<RoleBO>();
                            RoleBO roleModer = roleBO.LoadAll().FirstOrDefault(r => r.RoleName.Contains("moder"));
                            if (roleModer == null)
                            {
                                roleBO = IsRole(roleBO, "moder");
                            }
                            roleAccountBO.Role = roleModer;
                            roleAccountBO.AccountId = (int)accountBO.Id;
                            roleAccountBO.Save(roleAccountBO);
                        }
                    }

                    //if (accountBO != null)
                    //{
                    FormsAuthentication.SetAuthCookie(model.Login, true);

                    //далее accountId, roles, isActive будут храниться в КЛИЕНТЕ
                    var accountId = accountBO.Id;
                    var isActive = accountBO.IsActive;
                    roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                    var roles = roleAccountBOList.Select(r => r.Role.RoleName).ToList();

                    //2) сохр. в Session Server
                    HttpContext.Session["accountId"] = accountId;
                    return Json(new { success = true, message = "Wellcome!", accountId, isActive, roles }); //уйдет в предст. Login.html -> _Layout.html
                    //}
                    //else
                    //{
                    //    return Json(new { success = false, message = "Пользователя с таким логином и паролем нет" });
                    //}
                }
            }
            return Json(new { success = false, message = "Модель не валидна!" });
        }
        //------------------------------------------Registration ---------------------------------------------------------------------
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var accountBO = DependencyResolver.Current.GetService<AccountBO>();
                accountBO = accountBO.LoadAll().Where(u => u.Email != null && u.FullName != null).FirstOrDefault(u => u.Email == model.Email || u.FullName.Equals(model.FullName));
                if (accountBO == null)
                {
                    accountBO = CreateAccount(model);
                    if (accountBO != null)
                    {
                        FormsAuthentication.SetAuthCookie(model.FullName, true);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь с таким логином или именем уже существует");
                }
            }
            return View(model);
        }

        private AccountBO CreateAccount(RegisterModel model)
        {
            AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>();
            var roleBO = DependencyResolver.Current.GetService<RoleBO>();
            roleBO = roleBO.LoadAll().FirstOrDefault(r => r.RoleName.Contains("client"));
            roleBO = IsRole(roleBO, "client");    //проверка, если надо-установ.

            //1-create Address
            //var address = mapper.Map<AddressBO>(model);

            //2)созд. account
            accountBO = mapper.Map<AccountBO>(model);
            accountBO.IsActive = true;
            accountBO.Save(accountBO);

            int accountLastId = accountBO.GetLastId();
            accountBO = accountBO.Load(accountLastId);

            //3)теперь сохр. client
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO.AccountId = (int)accountBO.Id;  //accountLastId;
            clientBO.Save(clientBO);

            //4)роль юзера добавл. -  внес. изм. в табл. связей
            var roleAccountLinksBO = DependencyResolver.Current.GetService<RoleAccountLinkBO>();
            roleAccountLinksBO.RoleId = roleBO.Id;
            roleAccountLinksBO.AccountId = (int)accountBO.Id; //   
            roleAccountLinksBO.Save(roleAccountLinksBO);
            return accountBO.LoadAll().Where(u => u.FullName == model.FullName && u.Password == model.Password).FirstOrDefault();
        }

        private RoleBO IsRole(RoleBO roleBO, string param)
        {
            if (roleBO == null || roleBO.Id == 0 || !roleBO.RoleName.Equals(param))//если в БД нет роли client
            {
                roleBO = DependencyResolver.Current.GetService<RoleBO>();
                roleBO.RoleName = param;
                roleBO.Save(roleBO);
                roleBO = roleBO.LoadAll().Where(r => r.RoleName.Equals(param)).FirstOrDefault();  //получить уже с ID
            }
            return roleBO;

        }

        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
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
