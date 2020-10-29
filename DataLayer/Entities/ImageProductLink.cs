using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entities
{
    public class ImageProductLink
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("Image")]
        public int? ImageId { get; set; }
        public virtual Image Image { get; set; }

    }
}
