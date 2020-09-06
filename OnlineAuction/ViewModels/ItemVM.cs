using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class ItemVM
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual ProductVM Product { get; set; }

        public decimal EndPrice { get; set; }

        [Required]
        public int? OrderId { get; set; }
        public virtual OrderVM Order { get; set; }

    }
}