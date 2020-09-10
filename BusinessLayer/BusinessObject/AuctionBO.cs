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
    public class AuctionBO: BaseBusinessObject
    {
        public int Id { get; set; }
        
        public int ActorId { get; set; }
        public virtual ClientBO Actor { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
        
        public int ProductId { get; set; }
        public virtual ProductBO Product { get; set; }

        public decimal Step { get; set; }
        public decimal RedeptionPrice { get; set; }
        public string Description { get; set; }
        public ICollection<BetAuctionBO> BetAuctions { get; set; }
        
        public int WinnerId { get; set; }
        public ClientBO Winner { get; set; }

        public AuctionBO()
        {
            BeginTime = DateTime.Now;
        }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public AuctionBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<AuctionBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var auctions = unitOfWork.Auctions.GetAll();
            var res = auctions.AsEnumerable().Select(a => mapper.Map<AuctionBO>(a)).ToList();
            return res;
        }
        public AuctionBO Load(int id)
        {
            var auction = unitOfWork.Auctions.GetById(id);
            return mapper.Map(auction, this);
        }
        public void Save(AuctionBO auctionBO)
        {
            var auction = mapper.Map<Auction>(auctionBO);
            if (auctionBO.Id == 0) {
                Add(auction);
            }
            else {
                Update(auction);
            }
            unitOfWork.Auctions.Save();
        }
        private void Add(Auction auction)
        {
            unitOfWork.Auctions.Create(auction);
        }
        private void Update(Auction auction)
        {
            unitOfWork.Auctions.Update(auction);
        }
        public void DeleteSave(AuctionBO auctionBO)
        {
            var auction = mapper.Map<Auction>(auctionBO);
            unitOfWork.Auctions.Delete(auction.Id);
            unitOfWork.Auctions.Save();
        }
    }
}
