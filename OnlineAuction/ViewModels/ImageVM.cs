﻿using System;

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