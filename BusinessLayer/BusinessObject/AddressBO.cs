using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class AddressBO: BaseBusinessObject
    {
        public int? Id { get; set; }
        
        public string Region { get; set; }
        
        public string City { get; set; }
        
        public string Street { get; set; }

        
        public string House { get; set; }

        //---------------------------------------------------------------------------------
        readonly IUnityContainer unityContainer;
        public AddressBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<AddressBO> LoadAll()  //из DataObj в BusinessObj
        {
            var address = unitOfWork.Addresses.GetAll().ToList();
            var res = address.AsEnumerable().Select(a => mapper.Map<AddressBO>(a)).ToList();
            return res;
        }
        public IEnumerable<AddressBO> LoadAllNoTracking()  //из DataObj в BusinessObj
        {
            var address = unitOfWork.Addresses.GetAllNoTracking();
            var res = address.AsEnumerable().Select(a => mapper.Map<AddressBO>(a)).ToList();
            return res;
        }
        public IEnumerable<AddressBO> LoadAllWithInclude(params string[] properties)  //из DataObj в BusinessObj
        {
            var address = unitOfWork.Addresses.Include(properties);
            var res = address.AsEnumerable().Select(a => mapper.Map<AddressBO>(a)).ToList();
            return res;
        }
        public AddressBO Load(int id)
        {
            var address = unitOfWork.Addresses.GetById(id);
            return mapper.Map(address, this);
        }
        public void Save(AddressBO addressBO)
        {
            var address = mapper.Map<Address>(addressBO);
            if (addressBO.Id == 0 || address.Id == null) {    
                Add(address);
            }
            else {
                Update(address);
            }
            unitOfWork.Addresses.Save();
        }
        private void Add(Address address)
        {
            unitOfWork.Addresses.Create(address);
        }
        private void Update(Address address)
        {
            unitOfWork.Addresses.Update(address);
        }
        public void DeleteSave(AddressBO addressBO)
        {
            var address = mapper.Map<Address>(addressBO);
            unitOfWork.Addresses.Delete(address.Id);
            unitOfWork.Addresses.Save();
        }
    }
}
