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

        [ForeignKey("Actor")]
        public int ActorId { get; set; }
        public virtual Client Actor { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal Step { get; set; }

        public string Description { get; set; }

        public ICollection<BetAuction> BetAuctions { get; set; }

        [ForeignKey("Winner")]
        public int WinnerId { get; set; }
        public Client Winner { get; set; }

        public Auction()
        {
            BeginTime = DateTime.Now;
        }

    }
    
}
