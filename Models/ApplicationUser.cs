using Microsoft.AspNetCore.Identity;

namespace SistemApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Regular"; // "Admin" or "Regular"
        
        public string FullName => $"{FirstName} {LastName}";
    }
}