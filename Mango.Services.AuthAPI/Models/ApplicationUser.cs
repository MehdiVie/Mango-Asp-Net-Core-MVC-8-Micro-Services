using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
