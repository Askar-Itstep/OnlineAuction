using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entities
{
    public class Address
    {
        [Key]
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
        
    }
}
