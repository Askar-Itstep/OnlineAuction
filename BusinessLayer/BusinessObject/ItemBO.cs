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
    public class ItemBO: BaseBusinessObject
    {
        public int Id { get; set; }

        //в кажд. аукц. продукты униальны (даже с идентич. характерист.)
        public int ProductId { get; set; }
        public  ProductBO Product { get; set; }
        public decimal EndPrice { get; set; }

        public int? OrderId { get; set; }
        public OrderBO Order { get; set; }

        //-------------------------------------------

        readonly IUnityContainer unityContainer;
        public ItemBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<ItemBO> LoadAll()  //из DataObj в BusinessObj
        {
            var items = unitOfWork.Items.GetAll();
            var res = items.AsEnumerable().Select(a => mapper.Map<ItemBO>(a)).ToList();
            return res;
        }
        public ItemBO Load(int id)
        {
            var item = unitOfWork.Items.GetById(id);
            return mapper.Map(item, this);
        }
        public void Save(ItemBO ItemBO)
        {
            var item = mapper.Map<Item>(ItemBO);
            if (ItemBO.Id == 0) {
                Add(item);
            }
            else {
                Update(item);
            }
            unitOfWork.Items.Save();
        }
        private void Add(Item item)
        {
            unitOfWork.Items.Create(item);
        }
        private void Update(Item item)
        {
            unitOfWork.Items.Update(item);
        }
        public void DeleteSave(ItemBO itemBO)
        {
            var item = mapper.Map<Item>(itemBO);
            unitOfWork.Items.Delete(item.Id);
            unitOfWork.Items.Save();
        }
    }
}
