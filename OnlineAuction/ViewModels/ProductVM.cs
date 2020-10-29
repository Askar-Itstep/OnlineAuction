using AutoMapper;
using DataLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Unity;

namespace OnlineAuction.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public decimal Price { get; set; }

        public int? ImageId { get; set; }
        public virtual ImageVM Image { get; set; }
        
        public int? CategoryId { get; set; }
        public virtual CategoryVM Category { get; set; }
    }
}
