using AutoMapper;
using DataLayer.Entities;
using DataLayer.Repository;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace BusinessLayer.BusinessObject
{
    public class ProductBO : BaseBusinessObject
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public decimal Price { get; set; }

        public int? ImageId { get; set; }
        public virtual ImageBO Image { get; set; }

        public int? CategoryId { get; set; }
        public virtual CategoryBO Category { get; set; }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public ProductBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<ProductBO> LoadAll()  //из DataObj в BusinessObj
        {
            var products = unitOfWork.Products.GetAll();
            var res = products.AsEnumerable().Select(a => mapper.Map<ProductBO>(a)).ToList();
            return res;
        }
        public ProductBO Load(int id)
        {
            var product = unitOfWork.Products.GetById(id);
            return mapper.Map(product, this);
        }
        public ProductBO LoadAsNoTracking(int id)
        {
            var product = unitOfWork.Products.GetAllNoTracking().FirstOrDefault(p => p.Id == id);
            return mapper.Map(product, this);
        }
        public void Save(ProductBO productBO)
        {
            var product = mapper.Map<Product>(productBO);
            if (productBO.Id == 0)
            {
                Add(product);
            }
            else
            {
                Update(product);
            }
            unitOfWork.Products.Save();
        }
        private void Add(Product product)
        {
            unitOfWork.Products.Create(product);
        }
        private void Update(Product product)
        {
            unitOfWork.Products.Update(product);
        }
        public void DeleteSave(ProductBO productBO)
        {
            var product = mapper.Map<Product>(productBO);
            unitOfWork.Products.Delete(product.Id);
            unitOfWork.Products.Save();
        }

        public IEnumerable<ProductBO> LoadAllWithInclude(params string[] keys)
        {
            var products = unitOfWork.Products.Include(keys);
            var res = products.AsEnumerable().Select(a => mapper.Map<ProductBO>(a)).ToList();
            return res;
        }
    }
}
