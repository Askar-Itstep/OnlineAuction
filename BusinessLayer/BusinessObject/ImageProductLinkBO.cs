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
    public class ImageProductLinkBO : BaseBusinessObject
    {
        public int Id { get; set; }
        
        public int? ProductId { get; set; }
        public virtual ProductBO Product { get; set; }
        
        public int? ImageId { get; set; }
        public virtual ImageBO Image { get; set; }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public ImageProductLinkBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<ImageProductLinkBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var imageProductLinksBO = unitOfWork.ImageProductLinks.GetAll();
            var res = imageProductLinksBO.AsEnumerable().Select(a => mapper.Map<ImageProductLinkBO>(a)).ToList();
            return res;
        }
        public IEnumerable<ImageProductLinkBO> LoadAsNoTraking()  //из DataObj в BusinessObj 
        {
            var imageProductLinksBO = unitOfWork.ImageProductLinks.GetAllNoTracking();
            var res = imageProductLinksBO.AsEnumerable().Select(a => mapper.Map<ImageProductLinkBO>(a)).ToList();
            return res;
        }
        public ImageProductLinkBO Load(int id)
        {
            var imageProductLinkBO = unitOfWork.ImageProductLinks.GetById(id);
            return mapper.Map(imageProductLinkBO, this);
        }
        public void Save(ImageProductLinkBO imageProductLinkBO)
        {
            var imageProductLink = mapper.Map<ImageProductLink>(imageProductLinkBO);
            if (imageProductLink.Id == 0)
            {
                Add(imageProductLink);
            }
            else
            {
                Update(imageProductLink);
            }
            unitOfWork.ImageProductLinks.Save();
        }
        private void Add(ImageProductLink item)
        {
            unitOfWork.ImageProductLinks.Create(item);
        }
        private void Update(ImageProductLink item)
        {
            unitOfWork.ImageProductLinks.Update(item);
        }
        public void DeleteSave(ImageProductLinkBO itemBO)
        {
            var item = mapper.Map<ImageProductLink>(itemBO);
            unitOfWork.ImageProductLinks.Delete(item.Id);
            unitOfWork.ImageProductLinks.Save();
        }
    }
}
