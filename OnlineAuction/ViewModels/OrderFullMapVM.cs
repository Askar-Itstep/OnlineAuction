using BusinessLayer.BusinessObject;
using OnlineAuction.ServiceClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class OrderFullMapVM : ISyntetic
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public ClientVM Client { get; set; }

        public bool IsApproved { get; set; }

        //при подготов. Index.html - не используется (только при возвр. JSON из Index.html)
        public List<int> AuctionIds { get; set; }  

        public IEnumerable<DateTime> EndTimes { get; set; }

        //аналогич.
        public List<int> ProductIds { get; set; }
        public IEnumerable<ProductVM> Products { get; set; }

        public List<ItemVM> GetItems() 
        {
            List <ItemVM> items = new List<ItemVM>();
            Products.ToList().ForEach(p => items.Add(new ItemVM { Product = p }));
            return items;
        }
    }
}