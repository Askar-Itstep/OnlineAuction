using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineAuction.Entities
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey (name:"Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        [ForeignKey(name: "Order")]
        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }




    }
}