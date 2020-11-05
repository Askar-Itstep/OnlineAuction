using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class ClientBO : BaseBusinessObject
    {
        public int? Id { get; set; }
        public ICollection<OrderBO> Orders { get; set; }

        private decimal cash;

        public int AccountId { get; set; }
        public virtual AccountBO Account { get; set; }

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
            if (clientBO.Id == 0 || clientBO.Id == null)
            {
                Add(client);
            }
            else
            {
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

        public IEnumerable<ClientBO> LoadAllWithInclude(params string[] keys)
        {
            var clients = unitOfWork.Clients.Include(keys);
            var res = clients.AsEnumerable().Select(a => mapper.Map<ClientBO>(a)).ToList();
            return res;
        }
    }
}
