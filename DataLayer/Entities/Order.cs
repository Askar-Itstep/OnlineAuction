using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLayer.Entities
{
    public class Order
    {
        [Key]
        public int? Id { get; set; }
        
        public  virtual ICollection<Item> Items { get; set; }
       
        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public virtual Client Client { get; protected set; }


        public bool IsApproved { get; protected set; }

        public void AddItem(Item item)
        {
            item.Order = this;
            Items.Add(item);
        }
    }
}