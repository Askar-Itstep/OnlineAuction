using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace DataLayer.Entities
{
    public class BetAuction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Auction")]
        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        public decimal Bet { get; set; }
    }
}