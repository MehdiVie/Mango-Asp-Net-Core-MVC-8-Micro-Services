using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class LoginReequestDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
