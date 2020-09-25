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
    public class OrderBO: BaseBusinessObject
    {
        public int? Id { get; set; }

        public  ICollection<ItemBO> Items { get; set; }
                
        public int? ClientId { get; set; }
        public virtual ClientBO Client { get; protected set; }


        public bool IsApproved { get;  set; }

        public void AddItem(ItemBO itemBO)
        {
            itemBO.Order = this;
            Items.Add(itemBO);
        }
        

        //---------------------------
        readonly IUnityContainer unityContainer;
        public OrderBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<OrderBO> LoadAll()  //из DataObj в BusinessObj
        {
            var orders = unitOfWork.Orders.GetAll();
            var res = orders.AsEnumerable().Select(a => mapper.Map<OrderBO>(a)).ToList();
            return res;
        }
        public IEnumerable<OrderBO> LoadAllWithInclude(params string[] navigationProperty)  //из DataObj в BusinessObj
        {
            var orders = unitOfWork.Orders.Include(navigationProperty);
            var res = orders.AsEnumerable().Select(a => mapper.Map<OrderBO>(a)).ToList();
            return res;
        }
        public OrderBO Load(int id)
        {
            var order = unitOfWork.Orders.GetById(id);
            return mapper.Map(order, this);
        }
        public OrderBO LoadAsNoTracking(int id)
        {
            var order = unitOfWork.Orders.GetAllNoTracking().FirstOrDefault(o => o.Id == id);
            return mapper.Map(order, this);
        }
        public void Save(OrderBO orderBO)
        {
            var order = mapper.Map<Order>(orderBO);
            if (orderBO.Id == 0) {
                Add(order);
            }
            else {
                Update(order);
            }
            unitOfWork.Orders.Save();
        }
        private void Add(Order order)
        {
            unitOfWork.Orders.Create(order);
        }
        private void Update(Order order)
        {
            unitOfWork.Orders.Update(order);
        }
        public void DeleteSave(OrderBO orderBO)
        {
            //var client = mapper.Map<Account>(clientBO); //????????
            var order = mapper.Map<Order>(orderBO);
            unitOfWork.Orders.Delete(order.Id);
            unitOfWork.Orders.Save();
        }
    }
}
