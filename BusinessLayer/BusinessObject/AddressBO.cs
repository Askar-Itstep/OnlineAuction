using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.BusinessObject
{
    public class AddressBO
    {
        public int? Id { get; set; }
        
        public string Region { get; set; }
        
        public string City { get; set; }
        
        public string Street { get; set; }

        
        public string House { get; set; }
    }
}
