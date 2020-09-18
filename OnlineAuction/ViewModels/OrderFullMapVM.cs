using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class OrderFullMapVM
    {
        public int OrderId { get; set; }

        public ClientVM Client { get; set; }
        public bool IsApproved { get; set; }

        //public OrderAuctionsVM OrderAuctions {get; set;}

        public IEnumerable<int> AuctionIds { get; set; }
        public IEnumerable<ProductVM> Products { get; set; }

        //public OrderVM Order { get; set; }
    }
}