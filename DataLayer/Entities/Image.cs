using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.Entities
{
    public class Image
    {

        [Key]
        public int Id { get; set; }

        public string FileName { get; set; }

        public byte[] ImageData { get; set; }
    }
}