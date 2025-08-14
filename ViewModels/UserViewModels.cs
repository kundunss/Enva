using System.ComponentModel.DataAnnotations;

namespace SistemApp.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Regular";
    }

    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password (leave empty to keep current)")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Regular";
    }
}