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
using OnlineAuction.ViewModels;
using System.Web.Security;
using BusinessLayer.BusinessObject;

namespace OnlineAuction.Controllers
{
    public class AccountsController : Controller
    {
        private Model1 db = new Model1();
        
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Index()
        {
            return View(await db.Account.ToListAsync());
        }

        //[Authorize(Roles = "admin")]  //конроль роли перенесен в представл.-юзер может смотреть свой аккаунт!
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        //[Authorize(Roles = "admin")]  //конроль роли перенесен в представл.
        public ActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create
            //(FormCollection form)
            ([Bind(Include = "Id,FullName,Email,Password")] Account account) //,IsActive
        {
            if (ModelState.IsValid) { 
                //if (ModelState.IsValidField("FullName") && ModelState.IsValidField("Email") && ModelState.IsValidField("Password")) {
                //    string name = form.Get("FullName");
                //    string email = form.Get("Email");
                //    string password = form.Get("Password");
                ////bool is_active =  Boolean.Parse(form.Get("IsActive")); //бизнес-логика!
                //Account account = new Account(name, email, password);
                db.Account.Add(account);

                //получить данные по роли и юзеру
                int accountId = (int)db.Account.AsEnumerable().Last().Id;
                int roleId = db.Roles.FirstOrDefault(r => r.RoleName.Equals("member")).Id;  //11
                db.RoleAccountLinks.Add(new RoleAccountLink { RoleId = roleId, AccountId = accountId}); //12- переприсваивается???!
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
                return View();
        }

        //конроль роли перенесен в представл.
        public async Task<ActionResult> Edit(int? userId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(userId);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }
             
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit
        //(FormCollection form)
        ([Bind(Include = "Id,FullName,Email,Password,IsActive")] Account account)   //
        {
            if (ModelState.IsValid) {
            //if (ModelState.IsValidField("FullName") && ModelState.IsValidField("Email") && ModelState.IsValidField("Password") && ModelState.IsValidField("IsAcive")) { 
                //string name = form.Get("FullName");
                //string email = form.Get("Email");
                //string password = form.Get("Password");
                //bool is_active = Boolean.Parse(form.Get("IsActive")); //а здесь можно!            
                //Account account = new Account(name, email, password); //конструкт. больше нет!

                db.Entry(account).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return View(account);
            }
                return RedirectToAction("Index");
        }

        //конроль роли перенесен в представл.
        public async Task<ActionResult> Delete(int? userId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = await db.Account.FindAsync(userId);
            if (account == null)
            {
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
                var accountBOList = accountBO.LoadAll();
                accountBO = accountBOList.FirstOrDefault(u => (u.Email.Contains(model.Login) || u.Email.Contains(model.Login)) && u.Password.Equals(model.Password));

                if (accountBO != null) {
                    FormsAuthentication.SetAuthCookie(model.Login, true);
                    ////-----------после удаления azure - выгрузка отсутствует! ----------------
                    //var arrImgBackground = BlobHelper.DowloadUriBackground();
                    var accountId = accountBO.Id;
                    return Json(new { success = true, message = "Wellcome!", userId = accountId});//, image = uri, arrImg = arrImgBackground });
                }
                else
                    return Json(new { success = false, message = "Пользователя с таким логином и паролем нет" });
            }
            return Json(new { success = false, message = "Модель не валидна!" });
        }

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
                else
                    ModelState.AddModelError("", "Пользователь с таким логином или именем уже существует");
            }
            return View(model);
        }

        private AccountBO CreateAccount(RegisterModel model)
        {
            AccountBO accountBO = DependencyResolver.Current.GetService<AccountBO>();
            var roleBO = DependencyResolver.Current.GetService<RoleBO>();
            roleBO = roleBO.LoadAll().FirstOrDefault(r => r.RoleName.Contains("client"));
            roleBO = IsRole(roleBO, "client");    //проверка, если надо-установ.

            //1)созд. account
            accountBO.FullName = model.FullName;
            accountBO.Email = model.Email;
            accountBO.Password = model.Password;
            accountBO.Save(accountBO);    

            int accountLastId = accountBO.GetLastId();
            accountBO = accountBO.Load(accountLastId);

            //2)теперь сохр. client
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            clientBO.AccountId = (int)accountBO.Id;  //accountLastId;
            clientBO.Save(clientBO);

            //3)роль юзера добавл. -  внес. изм. в табл. связей
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
