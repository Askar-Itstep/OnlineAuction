using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineAuction.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public decimal Price { get; set; }

        [ForeignKey("Image")]
        public int? ImageId { get; set; }
        public virtual Image Image { get; set; }

    }
}