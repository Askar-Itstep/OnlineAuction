using Newtonsoft.Json;
using OnlineAuction.ServiceClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.ViewModels
{
    //[ModelBinder(typeof(AuctionViewModelBinder))]
    public class AuctionEditVM: ISyntetic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
       
        public decimal? Step { get; set; }
        
        //цена выкупа-по умолч. устан. в 300% от начальной
        public decimal RedemptionPrice { get; set; }
        
        public DateTime DayBegin { get; set; }
       
        public TimeSpan TimeBegin  { get; set; }
        
        public float Duration { get; set; }

        //public HttpPostedFileBase Upload { get; set; }

    }
}