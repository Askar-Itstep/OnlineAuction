using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class RoleAccountLinkBO : BaseBusinessObject
    {
        public int Id { get; set; }

        public int RoleId { get; set; }
        public virtual RoleBO Role { get; set; }

        public int AccountId { get; set; }
        public virtual AccountBO Account { get; set; }

        //-------------------------------------------
        readonly IUnityContainer unityContainer;
        public RoleAccountLinkBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<RoleAccountLinkBO> LoadAll()  //из DataObj в BusinessObj
        {
            var roleAccounts = unitOfWork.RoleAccountLinks.GetAll().ToList();
            var res = roleAccounts.AsEnumerable().Select(a => mapper.Map<RoleAccountLinkBO>(a)).ToList();
            return res;
        }
        public IEnumerable<RoleAccountLinkBO> LoadAllNoTracking()  //из DataObj в BusinessObj
        {
            var accounts = unitOfWork.RoleAccountLinks.GetAllNoTracking();
            var res = accounts.AsEnumerable().Select(a => mapper.Map<RoleAccountLinkBO>(a)).ToList();
            return res;
        }
        public IEnumerable<RoleAccountLinkBO> LoadAllWithInclude(params string[] properties)  //из DataObj в BusinessObj
        {
            var accounts = unitOfWork.RoleAccountLinks.Include(properties);
            var res = accounts.AsEnumerable().Select(a => mapper.Map<RoleAccountLinkBO>(a)).ToList();
            return res;
        }
        public RoleAccountLinkBO Load(int id)
        {
            var account = unitOfWork.RoleAccountLinks.GetById(id);
            return mapper.Map(account, this);
        }
        public void Save(RoleAccountLinkBO roleAccountLinkBO)
        {
            var roleAccountLink = mapper.Map<RoleAccountLink>(roleAccountLinkBO);
            if (roleAccountLink.Id == 0)
            {
                Add(roleAccountLink);
            }
            else
            {
                Update(roleAccountLink);
            }
            unitOfWork.RoleAccountLinks.Save();
        }
        private void Add(RoleAccountLink roleAccountLink)
        {
            unitOfWork.RoleAccountLinks.Create(roleAccountLink);
        }
        private void Update(RoleAccountLink roleAccountLink)
        {
            unitOfWork.RoleAccountLinks.Update(roleAccountLink);
        }
        public void DeleteSave(RoleAccountLinkBO roleAccountLinkBO)
        {
            var roleAccountLink = mapper.Map<RoleAccountLinkBO>(roleAccountLinkBO);
            unitOfWork.RoleAccountLinks.Delete(roleAccountLink.Id);
            unitOfWork.RoleAccountLinks.Save();
        }
    }
}
