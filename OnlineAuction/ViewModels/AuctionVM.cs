using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class AuctionVM
    {
        public int Id { get; set; }

        public int ActorId { get; set; }
        public virtual ClientVM Actor { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public int ProductId { get; set; }
        public virtual ProductVM Product { get; set; }

        public decimal Step { get; set; }
        public decimal RedemptionPrice { get; set; }
        public string Description { get; set; }

        public ICollection<BetAuctionVM> BetAuctions { get; set; }

        public int WinnerId { get; set; }
        public ClientVM Winner { get; set; }

        public AuctionVM()
        {
            BeginTime = DateTime.Now;
        }

        public bool IsActive { get; set; }
    }
}