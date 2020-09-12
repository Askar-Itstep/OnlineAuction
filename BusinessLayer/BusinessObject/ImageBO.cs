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
    public class ImageBO: BaseBusinessObject
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public byte[] ImageData { get; set; }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public ImageBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<ImageBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var images = unitOfWork.Images.GetAll();
            var res = images.AsEnumerable().Select(a => mapper.Map<ImageBO>(a)).ToList();
            return res;
        }
        public IEnumerable<ImageBO> LoadAsNoTraking()  //из DataObj в BusinessObj 
        {
            var images = unitOfWork.Images.GetAllNoTracking();
            var res = images.AsEnumerable().Select(a => mapper.Map<ImageBO>(a)).ToList();
            return res;
        }
        public ImageBO Load(int id)
        {
            var image = unitOfWork.Images.GetById(id);
            return mapper.Map(image, this);
        }
        public void Save(ImageBO imageBO)
        {
            var image = mapper.Map<Image>(imageBO);
            if (imageBO.Id == 0) {
                Add(image);
            }
            else {
                Update(image);
            }
            unitOfWork.Images.Save();
        }
        private void Add(Image item)
        {
            unitOfWork.Images.Create(item);
        }
        private void Update(Image item)
        {
            unitOfWork.Images.Update(item);
        }
        public void DeleteSave(ImageBO itemBO)
        {
            var item = mapper.Map<Image>(itemBO);
            unitOfWork.Images.Delete(item.Id);
            unitOfWork.Images.Save();
        }
    }
}
