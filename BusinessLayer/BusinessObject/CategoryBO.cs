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
    public class CategoryBO: BaseBusinessObject
    {
        public int Id { get; set; }

        public string Title { get; set; }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public CategoryBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<CategoryBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var categoriies = unitOfWork.Categories.GetAll();
            var res = categoriies.AsEnumerable().Select(a => mapper.Map<CategoryBO>(a)).ToList();
            return res;
        }
        public IEnumerable<CategoryBO> LoadAsNoTraking()  //из DataObj в BusinessObj 
        {
            var categoriies = unitOfWork.Categories.GetAllNoTracking();
            var res = categoriies.AsEnumerable().Select(a => mapper.Map<CategoryBO>(a)).ToList();
            return res;
        }
        public CategoryBO Load(int id)
        {
            var categoriy = unitOfWork.Categories.GetById(id);
            return mapper.Map(categoriy, this);
        }
        public void Save(CategoryBO categoryBO)
        {
            var category = mapper.Map<Category>(categoryBO);
            if (categoryBO.Id == 0)
            {
                Add(category);
            }
            else
            {
                Update(category);
            }
            unitOfWork.Categories.Save();
        }
        private void Add(Category item)
        {
            unitOfWork.Categories.Create(item);
        }
        private void Update(Category item)
        {
            unitOfWork.Categories.Update(item);
        }
        public void DeleteSave(CategoryBO itemBO)
        {
            var item = mapper.Map<Category>(itemBO);
            unitOfWork.Categories.Delete(item.Id);
            unitOfWork.Categories.Save();
        }
    }
}
