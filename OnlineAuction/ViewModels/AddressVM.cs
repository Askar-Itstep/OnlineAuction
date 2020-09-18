using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class AddressVM
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Region { get; set; }

        [Required]
        [StringLength(255)]
        public string City { get; set; }

        [Required]
        [StringLength(255)]
        public string Street { get; set; }


        [Required]
        [StringLength(255)]
        public string House { get; set; }


        public override string ToString()
        {
            return $"region: {Region}, city: {City}, street: {Street}, house: {House}";
        }
    }
}