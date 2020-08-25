using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OnlineAuction.Entities
{
    public class Order
    {
        [Key]
        public int? Id { get; set; }
        
        public virtual ICollection<Item> Items { get; set; }

        protected Order()
        {
            Items = new List<Item>();
        }

        public Order(IEnumerable<Item> items, Client client) : this()
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (client == null)
                throw new ArgumentNullException("client");

            Client = client;
            Items = items.ToList();
        }

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public virtual Client Client { get; protected set; }


        public bool IsApproved { get; protected set; }

        public void AddProduct(Item item)
        {
            item.Order = this;
            Items.Add(item);
        }
    }
}