using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class ImageVM
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        //public byte[] ImageData { get; set; }
        public Uri URI { get; set; }
    }
}