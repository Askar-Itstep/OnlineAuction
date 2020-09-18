using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class OrderVM
    {
        public int? Id { get; set; }

        public ICollection<ItemVM> Items { get; set; }
        
        public int? ClientId { get; set; }
        public virtual ClientVM Client { get; protected set; }


        public bool IsApproved { get; protected set; }

        public void AddItem(ItemVM item)
        {
            item.Order = this;
            Items.Add(item);
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Product.Price);
        }
    }
}