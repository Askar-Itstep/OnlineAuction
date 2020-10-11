using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class UserHubBO:BaseBusinessObject
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public virtual AccountBO Account { get; set; }

        public string ConnectionId { get; set; }

        //-----------------------------

        readonly IUnityContainer unityContainer;
        public UserHubBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<UserHubBO> LoadAll()  //из DataObj в BusinessObj
        {
            var userHubs = unitOfWork.UserHubs.GetAll().ToList();
            var res = userHubs.AsEnumerable().Select(a => mapper.Map<UserHubBO>(a)).ToList();
            return res;
        }
        public IEnumerable<UserHubBO> LoadAllNoTracking()  //из DataObj в BusinessObj
        {
            var userHubs = unitOfWork.UserHubs.GetAllNoTracking();
            var res = userHubs.AsEnumerable().Select(a => mapper.Map<UserHubBO>(a)).ToList();
            return res;
        }
        public IEnumerable<UserHubBO> LoadAllWithInclude(params string[] properties)  //из DataObj в BusinessObj
        {
            var userHubs = unitOfWork.UserHubs.Include(properties);
            var res = userHubs.AsEnumerable().Select(a => mapper.Map<UserHubBO>(a)).ToList();
            return res;
        }
        public UserHubBO Load(int id)
        {
            var userHubs = unitOfWork.UserHubs.GetById(id);
            return mapper.Map(userHubs, this);
        }
        public void Save(UserHubBO userHubsBO)
        {
            var userHubs = mapper.Map<UserHub>(userHubsBO);
            if (userHubsBO.Id == 0 || userHubs.Id == null) {
                Add(userHubs);
            }
            else {
                Update(userHubs);
            }
            unitOfWork.UserHubs.Save();
        }
        private void Add(UserHub userHubs)
        {
            unitOfWork.UserHubs.Create(userHubs);
        }
        private void Update(UserHub userHubs)
        {
            unitOfWork.UserHubs.Update(userHubs);
        }
        public void DeleteSave(UserHubBO userHubsBO)
        {
            var userHubs = mapper.Map<UserHub>(userHubsBO);
            unitOfWork.UserHubs.Delete(userHubs.Id);
            unitOfWork.UserHubs.Save();
        }
    }
}
