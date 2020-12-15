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
    public class BetAuctionBO: BaseBusinessObject
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public AuctionBO Auction { get; set; }

        public int ClientId { get; set; }
        public ClientBO Client { get; set; }

        public decimal Bet { get; set; }

        //------------------------------
        readonly IUnityContainer unityContainer;
        public BetAuctionBO(IMapper mapper, UnitOfWork unitOfWork, IUnityContainer container)
            : base(mapper, unitOfWork)
        {
            unityContainer = container;
        }
        public IEnumerable<BetAuctionBO> LoadAll()  //из DataObj в BusinessObj 
        {
            var betAuctions = unitOfWork.BetAuctions.GetAll();
            var res = betAuctions.AsEnumerable().Select(a => mapper.Map<BetAuctionBO>(a)).ToList();
            return res;
        }
        public IEnumerable<BetAuctionBO> LoadAllWithInclude(params string[] parameters)  //из DataObj в BusinessObj 
        {
            var betAuctions = unitOfWork.BetAuctions.Include(parameters);
            var res = betAuctions.AsEnumerable().Select(a => mapper.Map<BetAuctionBO>(a)).ToList();
            return res;
        }
        public BetAuctionBO Load(int id)
        {
            var betAuction = unitOfWork.BetAuctions.GetById(id);
            return mapper.Map(betAuction, this);
        }
        public void Save(BetAuctionBO betAuctionBO)
        {
            var betAuction = mapper.Map<BetAuction>(betAuctionBO);
            if (betAuctionBO.Id == 0) {
                Add(betAuction);
            }
            else {
                Update(betAuction);
            }
            unitOfWork.BetAuctions.Save();
        }
        public async Task SaveAsync(BetAuctionBO betAuctionBO)
        {
            var betAuction = mapper.Map<BetAuction>(betAuctionBO);
            if (betAuctionBO.Id == 0)
            {
                Add(betAuction);
            }
            else
            {
                Update(betAuction);
            }
            await unitOfWork.BetAuctions.SaveAsync();
        }
        private void Add(BetAuction betAuction)
        {
            unitOfWork.BetAuctions.Create(betAuction);
        }
        private void Update(BetAuction betAuction)
        {
            unitOfWork.BetAuctions.Update(betAuction);
        }
        public void DeleteSave(BetAuctionBO betAuctionBO)
        {
            var betAuction = mapper.Map<BetAuction>(betAuctionBO);
            unitOfWork.BetAuctions.Delete(betAuction.Id);
            unitOfWork.BetAuctions.Save();
        }

        public async Task<BetAuction> FindByIdAsync(int? id)
        {
            if (id is null) return null;
             return await unitOfWork.BetAuctions.GetByIdAsync((int)id);            
        }
        public  async Task<BetAuctionBO> FindBOByIdAsync(int? id)
        {
            if (id is null) return null;
            BetAuction betAuction =await unitOfWork.BetAuctions.GetByIdAsync((int)id);
            return mapper.Map<BetAuctionBO>(betAuction);
        }
    }
}
