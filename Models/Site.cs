using System.ComponentModel.DataAnnotations;

namespace SistemApp.Models
{
    public class Site
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Site name is required")]
        [Display(Name = "Site Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Firma seçimi gereklidir")]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }
        
        // Navigation properties
        public Company? Company { get; set; }
        public ICollection<Personnel>? Personnel { get; set; }
        public ICollection<SystemHardware>? SystemHardware { get; set; }
        public ICollection<MailEntry>? MailEntries { get; set; }
        public ICollection<InventoryItem>? InventoryItems { get; set; }
    }
}