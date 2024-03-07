using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCardAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode{ get; set;}
        [NotMapped]
        public int Discount { get; set;}
        [NotMapped]
        public int CartTotal { get; set; }
    }
}
