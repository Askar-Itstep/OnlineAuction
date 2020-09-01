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
    public class AuctionClientsLinkBO: BaseBusinessObject
    {
        public int Id { get; set; }
        
        public int AuctionId { get; set; }
        public virtual AuctionBO Auction { get; set; }
        
        public int ClientId { get; set; }
        public virtual ClientBO Client { get; set; }


        //------------------------------
        readonly IUnityContainer unityContainer;
        public AuctionClientsLinkBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<AuctionClientsLinkBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var auctionClientLinks = unitOfWork.AuctionClientsLinks.GetAll();
            var res = auctionClientLinks.AsEnumerable().Select(a => mapper.Map<AuctionClientsLinkBO>(a)).ToList();
            return res;
        }
        public AuctionClientsLinkBO Load(int id)
        {
            var auctionClientLink = unitOfWork.AuctionClientsLinks.GetById(id);
            return mapper.Map(auctionClientLink, this);
        }
        public void Save(AuctionClientsLinkBO auctionClientLinkBO)
        {
            var auctionClientLink = mapper.Map<AuctionClientsLink>(auctionClientLinkBO);
            if (auctionClientLinkBO.Id == 0) {
                Add(auctionClientLink);
            }
            else {
                Update(auctionClientLink);
            }
            unitOfWork.AuctionClientsLinks.Save();
        }
        private void Add(AuctionClientsLink auctionClientLink)
        {
            unitOfWork.AuctionClientsLinks.Create(auctionClientLink);
        }
        private void Update(AuctionClientsLink auctionClientLink)
        {
            unitOfWork.AuctionClientsLinks.Update(auctionClientLink);
        }
        public void DeleteSave(AuctionClientsLinkBO auctionClientLinkBO)
        {
            var auctionClientLink = mapper.Map<AuctionClientsLink>(auctionClientLinkBO);
            unitOfWork.AuctionClientsLinks.Delete(auctionClientLink.Id);
            unitOfWork.AuctionClientsLinks.Save();
        }
    }
}
