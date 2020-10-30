using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class ImageProductLinkVM
    {
        public int Id { get; set; }
               
        public int? ProductId { get; set; }
        public virtual ProductVM Product { get; set; }

       
        public int? ImageId { get; set; }
        public virtual ImageVM Image { get; set; }
    }
}