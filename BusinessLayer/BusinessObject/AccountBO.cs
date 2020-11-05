using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class AccountBO : BaseBusinessObject
    {

        #region simple fields
        public int? Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }

        public int ImageId { get; set; }
        public virtual ImageBO Image { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public int AddressId { get; set; }
        public virtual AddressBO Address { get; set; }

        public ICollection<RoleBO> RolesBO { get; set; }

        public Gender Gender { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; private set; } = 0;

        public DateTime CreateAt { get; set; }
        public DateTime RemoveAt { get; set; }
        public void AddBalance(decimal value)
        {
            Balance += value;
        }
        public void AddRole(RoleBO roleBO)
        {
            if (RolesBO.Contains(roleBO))
            {
                return;
            }

            RolesBO.Add(roleBO);
        }
        public void RemoveRole(RoleBO role)
        {
            if (!role.RoleName.Contains("admin") && RolesBO != null)
            {
                if (RolesBO.Contains(role))
                {
                    RolesBO.Remove(role);
                }
            }
            return;
        }
        #endregion

        //---------------------------------------------------------------------------------
        readonly IUnityContainer unityContainer;
        public AccountBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<AccountBO> LoadAll()  //из DataObj в BusinessObj
        {
            var accounts = unitOfWork.Accounts.GetAll().ToList();
            var res = accounts.AsEnumerable().Select(a => mapper.Map<AccountBO>(a)).ToList();
            return res;
        }
        public IEnumerable<AccountBO> LoadAllNoTracking()  //из DataObj в BusinessObj
        {
            var accounts = unitOfWork.Accounts.GetAllNoTracking();
            var res = accounts.AsEnumerable().Select(a => mapper.Map<AccountBO>(a)).ToList();
            return res;
        }
        public IEnumerable<AccountBO> LoadAllWithInclude(params string[] properties)  //из DataObj в BusinessObj
        {
            var accounts = unitOfWork.Accounts.Include(properties);
            var res = accounts.AsEnumerable().Select(a => mapper.Map<AccountBO>(a)).ToList();
            return res;
        }
        public AccountBO Load(int id)
        {
            var account = unitOfWork.Accounts.GetById(id);
            return mapper.Map(account, this);
        }
        public void Save(AccountBO accountBO)
        {
            var account = mapper.Map<Account>(accountBO);
            if (accountBO.Id == 0 || account.Id == null)
            {    //почему Null??????????????
                Add(account);
            }
            else
            {
                Update(account);
            }
            unitOfWork.Accounts.Save();
        }
        private void Add(Account account)
        {
            unitOfWork.Accounts.Create(account);
        }
        private void Update(Account account)
        {
            unitOfWork.Accounts.Update(account);
        }
        public void DeleteSave(AccountBO accountBO)
        {
            var account = mapper.Map<Account>(accountBO);
            unitOfWork.Accounts.Delete(account.Id);
            unitOfWork.Accounts.Save();
        }

        //--------------см. стр 45 BaseRepo-----------
        public int GetLastId()
        {
            return (int)unitOfWork.Accounts.GetLast().Id;
        }

    }
}
