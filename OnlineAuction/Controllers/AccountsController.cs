using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
            this.mapper = mapper; //вызов карт сопостовл. зависимостей
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
            ViewBag.DateNull = 0;
            if (account.RemoveAt.ToString("d").Contains("3000"))   //GetDateTimeFormats("d"))
            {
                ViewBag.DateNull = 1;
            }
            return View(account);
        }


        //контроль роли  в представл.-_Layout
        //Изм. аккаун. по: id - для админа, accountId - для user'a
        //flag - ключ ajax из _Layout.html для пополн. баланс
        public ActionResult Edit(int? id, int? flag)
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
                if (flag == null)   //редакт. свой аккаунт
                {
                    return View(mapper.Map<AccountVM>(account));
                }
                else    // изм. баланса //->ajax/_Layout.html
                {
                    //ViewBag.Genders = 
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
        public async Task<ActionResult> Edit(AccountVM account, HttpPostedFileBase upload, decimal? balance)
        {
            if (account.Id != null)
            { //при простом изм. аккаунта
                if (ModelState.IsValid)
                {
                    //CreateAt, RemoveAt - здесь const!
                    var accountId = (int)Session["accountId"];
                    AccountBO accountBaseBO = DependencyResolver.Current.GetService<AccountBO>().Load(accountId);
                    DateTime dateCreate = accountBaseBO.CreateAt;
                    DateTime dateRemove = accountBaseBO.RemoveAt;

                    //при изм.адр. -адр. не добвал. в БД, а измен.
                    AddressVM address = account.Address;
                    AddressBO addressBO = mapper.Map<AddressBO>(address);
                    addressBO.Save(addressBO);

                    AccountBO accountBO = mapper.Map<AccountBO>(account);
                    accountBO.CreateAt = dateCreate;
                    accountBO.RemoveAt = dateRemove;
                    //изображение
                    if (upload != null) 
                    {
                        ImageVM imageVM = DependencyResolver.Current.GetService<ImageVM>();
                        ImageBO imageBase = DependencyResolver.Current.GetService<ImageBO>();
                        var imgBase = await BlobHelper.SetImageAsync(upload, imageVM, imageBase, mapper, accountBO);//если такого нет - записать в БД и вернуть! 
                    }                  
                    accountBO.Save(accountBO);  //address -> cascade?
                    return new JsonResult { Data = "Данные записаны", JsonRequestBehavior = JsonRequestBehavior.DenyGet };
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

                RoleAccountLinkVM linkVM = new RoleAccountLinkVM { AccountId = accountId, RoleId = moderRole.Id };
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
        public ActionResult DeleteConfirmed(int id)
        {
            AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>().Load(id);
            //нужно не удалять, а помечать как удаленный!
            accountBO.IsActive = false;
            accountBO.RemoveAt = DateTime.Now;
            accountBO.Save(accountBO);
            return RedirectToAction("Index");
        }
        //------------------------------------L.O.G.I.N.-----------------------------------------------------------------------
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

                var accountBOList = accountBO.LoadAll().ToList();
                var findAccount =accountBOList.Where(a => a.IsActive); //только действующие аккаунты
                accountBO = accountBOList.FirstOrDefault(u =>
                            (u.Email.Contains(model.Login) || u.Email.Contains(model.Login)) && u.Password.Equals(model.Password));
                if (accountBO is null)
                {
                    return new JsonResult { Data = new { success = false, message = "Пользователя с таким логином и паролем нет. Либо ваш аккаунт заблокирован" }, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
                }
                var roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                //accountBO.RolesBO = rolesBO;

                //нет денег удал. роль moder -> удалить или добавитьсвязь Role-Account
                RoleAccountLinkBO linkRoleClientAccount = roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault();    //client- authoregistr.
                if (accountBO.Balance <= 0)
                { 
                    if (linkRoleClientAccount != null)
                    {
                        roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault().DeleteSave(linkRoleClientAccount);
                    }
                }
                else//..если есть деньги, но роль "модер" удалена или не было
                {
                    if (linkRoleClientAccount == null)
                    {
                        RoleBO roleBO = DependencyResolver.Current.GetService<RoleBO>();
                        RoleBO roleModer = roleBO.LoadAll().FirstOrDefault(r => r.RoleName.Contains("moder"));
                        if (roleModer == null)  //проверить и добавить
                        {
                            roleBO = IsRole(roleBO, "moder");
                        }
                        roleAccountBO.RoleId = roleModer.Id;
                        roleAccountBO.AccountId = (int)accountBO.Id;
                        roleAccountBO.Save(roleAccountBO);
                        //перезагрузить роли-аккаунты
                        roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                    }
                }
                List<RoleBO> rolesBO = roleAccountBOList.Where(r => r.AccountId == accountBO.Id).Select(r => r.Role).ToList();
                FormsAuthentication.SetAuthCookie(model.Login, true);

                //далее accountId,  myURI, myRoles отправл.по ajax в КЛИЕНТ
                var accountId = accountBO.Id;
                var myURI = accountBO.Image.URI;
                var isAdmin = roleAccountBOList.Select(r => r.Role.RoleName.ToLower().Contains("admin")).FirstOrDefault();
                IEnumerable<string> myRoles = rolesBO.Select(r => r.RoleName);

                //2) сохр. в Session Server
                HttpContext.Session["accountId"] = accountId;
                HttpContext.Session["isAdmin"] = isAdmin;
                return new JsonResult
                {
                    Data = new { success = true, message = "Wellcome!", accountId, myURI, myRoles }
                                                                    ,
                    JsonRequestBehavior = JsonRequestBehavior.DenyGet
                };
            }
            return new JsonResult { Data = new { success = false, message = "Модель не валидна!" }, JsonRequestBehavior = JsonRequestBehavior.DenyGet };
        }
        //------------------------------------------Registration ---------------------------------------------------------------------
        [Authorize(Roles = "admin")] //------blocking -----------
        public ActionResult Registration()
        {
            return View();
        }
          
        [Authorize(Roles = "admin")]
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

            //1 Address - по умолч.,ID будет новым,т.к. у кажд.акканта свой адрес (даже если одинаков.) и при изм. полей ID неизм.            
            //2)созд. account
            accountBO = mapper.Map<AccountBO>(model);
            accountBO.ImageId = DependencyResolver.Current.GetService<ImageBO>().LoadAll().ToList().First().Id;
            var addressBO = DependencyResolver.Current.GetService<AddressBO>().LoadAll().ToList().First();
            accountBO.Address = addressBO;
            accountBO.AddressId = (int)addressBO.Id;
            //3
            accountBO.IsActive = true;
            accountBO.CreateAt = DateTime.Now;
            accountBO.RemoveAt = DateTime.Parse("3000-01-01 00:00:00");
            accountBO.Save(accountBO);

            int accountLastId = accountBO.GetLastId();
            accountBO = accountBO.Load(accountLastId);

            //4)теперь сохр. client
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO.AccountId = (int)accountBO.Id;
            clientBO.Save(clientBO);

            //4=5)роль юзера добавл. -  внес. изм. в табл. связей
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
            return RedirectToAction("Login", "Accounts");
        }

    }
}
