using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class JsonRequestCreateBet
    {
        public string AuctionId { get; set; }
        public string ClientId { get; set; }
        public string Bet { get; set; }
    }
}