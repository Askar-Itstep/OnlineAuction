using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class ClientBO: BaseBusinessObject
    {
        public int? Id { get; set; }
        public ICollection<OrderBO> Orders { get; set; }
        
        private decimal cash;
        
        public int AccountId { get;  set; }
        public virtual AccountBO Account { get; set; }

        //protected ClientBO()
        //{
        //    OrdersBO = new List<OrderBO>();
        //}

        //public ClientBO(AccountBO accountBO) : this()
        //{
        //    AccountBO = accountBO ?? throw new ArgumentNullException("accountBO");
        //    var clientBO = DependencyResolver.Current.GetService<RoleBO>();
        //    clientBO.LoadAll().Where(r => r.RoleName.ToLower().Equals("client")).FirstOrDefault();
        //    AccountBO.AddRole(clientBO);
        //    cash = 0;
        //}

        //клиент может быть должником (услуги в кредит?)

        public bool IsDebitor()
        {
            return cash < 0 ? true : false;
        }

        //а вхоДн. сумма отрицательной?
        public void AddCash(decimal sum)
        {
            cash += sum;
        }

        readonly IUnityContainer unityContainer;
        public ClientBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<ClientBO> LoadAll()  //из DataObj в BusinessObj
        {
            var clients = unitOfWork.Clients.GetAll();
            var res = clients.AsEnumerable().Select(a => mapper.Map<ClientBO>(a)).ToList();
            return res;
        }
        public ClientBO Load(int id)
        {
            var client = unitOfWork.Clients.GetById(id);
            return mapper.Map(client, this);
        }
        public void Save(ClientBO clientBO)
        {
            var client = mapper.Map<Client>(clientBO);
            if (clientBO.Id == 0 || clientBO.Id == null) {
                Add(client);
            }
            else {
                Update(client);
            }
            unitOfWork.Clients.Save();
        }
        private void Add(Client client)
        {
            unitOfWork.Clients.Create(client);
        }
        private void Update(Client client)
        {
            unitOfWork.Clients.Update(client);
        }
        public void DeleteSave(ClientBO clientBO)
        {
            var client = mapper.Map<Client>(clientBO);
            unitOfWork.Clients.Delete(client.Id);
            unitOfWork.Clients.Save();
        }
    }
}
