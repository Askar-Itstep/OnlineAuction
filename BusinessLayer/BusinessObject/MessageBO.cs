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
    public class MessageBO: BaseBusinessObject
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ClientBO Client { get; set; }
        
        public int PartnerId { get; set; }
        public virtual ClientBO Partner { get; set; }
        
        public string Sms { get; set; }


        //------------------------------
        readonly IUnityContainer unityContainer;
        public MessageBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<MessageBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var messages = unitOfWork.Messages.GetAll();
            var res = messages.AsEnumerable().Select(a => mapper.Map<MessageBO>(a)).ToList();
            return res;
        }
        public MessageBO Load(int id)
        {
            var message = unitOfWork.Messages.GetById(id);
            return mapper.Map(message, this);
        }
        public MessageBO Save(MessageBO messageBO)
        {
            var message = mapper.Map<Message>(messageBO);
            if (messageBO.Id == 0) {
                Add(message);
            }
            else {
                Update(message);
            }
            unitOfWork.Messages.Save();
            return messageBO;
        }
        private void Add(Message message)
        {
            unitOfWork.Messages.Create(message);
        }
        private void Update(Message message)
        {
            unitOfWork.Messages.Update(message);
        }
        public void DeleteSave(MessageBO messageBO)
        {
            var message = mapper.Map<Message>(messageBO);
            unitOfWork.Messages.Delete(message.Id);
            unitOfWork.Messages.Save();
        }
    }
}
