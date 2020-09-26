﻿using AutoMapper;
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

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Index()
        {
            return View(await db.Account.ToListAsync());
        }


        //[Authorize(Roles = "admin")]  //контроль роли перенесен в представл.-юзер может смотреть свой аккаунт!
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(id);
            if (account == null) {
                return HttpNotFound();
            }
            return View(account);
        }


        #region oldCreate
        ////[Authorize(Roles = "admin")]  
        //public ActionResult Create()
        //{
        //    //return View();
        //    return RedirectToAction("Registration");
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(Account account)
        //{
        //    if (ModelState.IsValid) {
        //        db.Account.Add(account);
        //        //получить данные по роли и юзеру
        //        int accountId = (int)db.Account.AsEnumerable().Last().Id;
        //        int roleId = db.Roles.FirstOrDefault(r => r.RoleName.Equals("member")).Id;  //11
        //        db.RoleAccountLinks.Add(new RoleAccountLink { RoleId = roleId, AccountId = accountId }); //12- переприсваивается???!
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    else {
        //        return View();
        //    }
        //}
        #endregion

        //контроль роли  в представл.-_Layout
        //id - для админа, accountId - для user'a
        //flag - ключ из _Layout.html для выбора: редакт. акк. или пополн. баланс
        public async Task<ActionResult> Edit(int? id, int? flag)
        {
            //----------для юзера---------------
            if (id == null) {
                var accountId = Session["accountId"] ?? 0;

                if ((int)accountId == 0) { //обяз. перелогин.
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                //Account account = await db.Account.FindAsync((int)accountId);
                AccountBO account = DependencyResolver.Current.GetService<AccountBO>().Load((int)accountId);
                if (account == null) {
                    return HttpNotFound();
                }
                if (flag == null) {
                    return View(mapper.Map<AccountVM>(account));
                }
                else {
                    return new JsonResult { Data = account, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            //---------для админа--------------
            else {
                //Account account = await db.Account.FindAsync(id);
                AccountBO account = DependencyResolver.Current.GetService<AccountBO>().Load((int)id);
                if (account == null) {
                    return HttpNotFound();
                }
                return View(mapper.Map<AccountVM>(account));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Account account, decimal? balance)
        {
            if (account.Id != null) { //при простом изм. аккаунта
                if (ModelState.IsValid) {
                    account.Address.Id = account.AddressId;
                    var address = account.Address;
                    db.Entry(address).State = EntityState.Modified;

                    db.Entry(account).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return View(account);
                }
            }
            else if (balance != null) { //при изм. баланса - добав. роль <Moder>(созд. лота)
                var accountId = (int)Session["accountId"];
                account = await db.Account.FindAsync(accountId);
                account.Balance = (decimal)balance;

                var rolesAccountLinks = db.RoleAccountLinks.Where(r => r.AccountId == account.Id).ToList();
                List<Role> roles = rolesAccountLinks.Select(r => r.Role).ToList();

                //account.SetRoles(roles); //over code-no save!
                var moderRole = db.Roles.Where(r => r.RoleName.Contains("moder")).FirstOrDefault();
                db.RoleAccountLinks.Add(new RoleAccountLink { AccountId = (int)account.Id, RoleId = moderRole.Id });

                db.Entry(account).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


        //контроль роли перенесен в представл.
        public async Task<ActionResult> Delete(int? id) //id del client, not user-admin
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(id);
            if (account == null) {
                return HttpNotFound();
            }
            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Account account = await db.Account.FindAsync(id);
            db.Account.Remove(account);
            //db.RoleAccountLinks.Remove() - не нужно, БД следует по каскаду!
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
            if (ModelState.IsValid) {
                var accountBO = DependencyResolver.Current.GetService<AccountBO>();
                var roleAccountBO = DependencyResolver.Current.GetService<RoleAccountLinkBO>();

                var accountBOList = accountBO.LoadAll();
                accountBO = accountBOList.FirstOrDefault(u => (u.Email.Contains(model.Login) || u.Email.Contains(model.Login)) && u.Password.Equals(model.Password));
                if (accountBO == null) {
                    return RedirectToAction("Index", "Home");
                }

                var roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                List<RoleBO> rolesBO = roleAccountBOList.Where(r => r.AccountId == accountBO.Id).Select(r => r.Role).ToList();
                accountBO.RolesBO = rolesBO;

                //связь котор. надо удалить или добавить
                RoleAccountLinkBO linkRoleClientAccount = roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault();    //client
                if (accountBO.Balance <= 0) { //удал. роль клиента

                    if (linkRoleClientAccount != null) {
                        roleAccountBOList.Where(r => r.Role.RoleName.Contains("moder")).FirstOrDefault().DeleteSave(linkRoleClientAccount);
                    }
                }
                else {
                    if (linkRoleClientAccount == null) {
                        RoleBO roleModer = DependencyResolver.Current.GetService<RoleBO>().LoadAll().FirstOrDefault(r => r.RoleName.Contains("moder"));
                        roleAccountBO.Role = roleModer;
                        roleAccountBO.Account = accountBO;
                        roleAccountBO.Save(roleAccountBO);
                    }
                }

                if (accountBO != null) {
                    FormsAuthentication.SetAuthCookie(model.Login, true);

                    //КЛЮЧЕВОЙ МОМЕНТ: далее accountId, roles, isActive будут храниться в КЛИЕНТЕ
                    var accountId = accountBO.Id;
                    var isActive = accountBO.IsActive;
                    roleAccountBOList = roleAccountBO.LoadAll().Where(r => r.AccountId == accountBO.Id).ToList();
                    var roles = roleAccountBOList.Select(r => r.Role.RoleName).ToList();
                    //2) сохр. в Sever-Session
                    HttpContext.Session["accountId"] = accountId;
                    return Json(new { success = true, message = "Wellcome!", accountId, isActive, roles }); //уйдет в предст. Login.html -> _Layout.html
                }
                else {
                    return Json(new { success = false, message = "Пользователя с таким логином и паролем нет" });
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
            if (ModelState.IsValid) {
                var accountBO = DependencyResolver.Current.GetService<AccountBO>();
                accountBO = accountBO.LoadAll().Where(u => u.Email != null && u.FullName != null).FirstOrDefault(u => u.Email == model.Email || u.FullName.Equals(model.FullName));
                if (accountBO == null) {
                    accountBO = CreateAccount(model);
                    if (accountBO != null) {
                        FormsAuthentication.SetAuthCookie(model.FullName, true);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else {
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
            var address = new Address { Region = model.Region, City = model.City, Street = model.Street, House = model.House };
            db.Addresses.Add(address);

            //2)созд. account
            accountBO.FullName = model.FullName;
            accountBO.Email = model.Email;
            accountBO.Password = model.Password;
            accountBO.Address = mapper.Map<AddressBO>(address);
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
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
