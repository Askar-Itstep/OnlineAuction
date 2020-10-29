using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.BusinessObject
{
    public class ImageProductLinkBO
    {
        public int Id { get; set; }
        
        public int? ProductId { get; set; }
        public virtual ProductBO Product { get; set; }
        
        public int? ImageId { get; set; }
        public virtual ImageBO Image { get; set; }
    }
}
