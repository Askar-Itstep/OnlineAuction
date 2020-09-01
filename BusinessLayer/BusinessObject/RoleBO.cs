using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class RoleBO: BaseBusinessObject
    {
        public int Id { get; set; }

        public string RoleName { get; set; }

        readonly IUnityContainer unityContainer;
        public RoleBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<RoleBO> LoadAll()  //из DataObj в BusinessObj
        {
            var roles = unitOfWork.Roles.GetAll();
            var res = roles.AsEnumerable().Select(a => mapper.Map<RoleBO>(a)).ToList();
            return res;
        }
        public RoleBO Load(int id)
        {
            var role = unitOfWork.Roles.GetById(id);
            return mapper.Map(role, this);
        }
        public void Save(RoleBO roleBO)
        {
            var role = mapper.Map<Role>(roleBO);
            if (roleBO.Id == 0) {
                Add(role);
            }
            else {
                Update(role);
            }
            unitOfWork.Roles.Save();
        }
        private void Add(Role role)
        {
            unitOfWork.Roles.Create(role);
        }
        private void Update(Role role)
        {
            unitOfWork.Roles.Update(role);
        }
        public void DeleteSave(RoleBO roleBO)
        {
            var role = mapper.Map<Role>(roleBO);
            unitOfWork.Roles.Delete(role.Id);
            unitOfWork.Roles.Save();
        }
    }
}