using OnlineAuction.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entities
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Client")]
        public int ActorId { get; set; }
        public virtual Client Client { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [ForeignKey("Client")]
        public int WinnerId { get; set; }
        public virtual Client Winner { get; set; }

        public Auction()
        {
            BeginTime = DateTime.Now;
        }

    }

    //public class OrderAuctionLinks
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    [ForeignKey("Order")]
    //    public int OrderId { get; set; }
    //    public virtual Order Order { get; set; }

    //    [ForeignKey("Auction")]
    //    public int AuctionId { get; set; }
    //    public virtual Auction Auction { get; set; }
    //}
}
