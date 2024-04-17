using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class RegisterationReequestDto
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [DataType(DataType.EmailAddress)]
        [Compare("Email")]
        public string ReEmail { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string RePassword { get; set; }
        [Required]
        public string Name { get; set; }
        
        public string PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
