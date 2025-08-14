using System.ComponentModel.DataAnnotations;

namespace SistemApp.Models
{
    public class Company
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Firma adı gereklidir")]
        [Display(Name = "Firma Adı")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
        
        // Navigation properties
        public ICollection<Site>? Sites { get; set; }
        public ICollection<SystemHardware>? SystemHardware { get; set; }
    }
}