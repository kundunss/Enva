using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class SystemHardware
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Equipment type is required")]
        [Display(Name = "Equipment Type")]
        public int EquipmentTypeId { get; set; }
        
        [ForeignKey("EquipmentTypeId")]
        public EquipmentType? EquipmentType { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "IP Address is required")]
        [Display(Name = "IP Address")]
        public string IpAddress { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;
        
        [Display(Name = "Password")]
        public string? Password { get; set; } // nullable
        
        [Required(ErrorMessage = "Firma seçimi gereklidir")]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }
        
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
        
        [Required(ErrorMessage = "Site is required")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        
        [ForeignKey("SiteId")]
        public Site? Site { get; set; }
        
        // Navigation properties
        public ICollection<PasswordHistory>? PasswordHistory { get; set; }
    }
}