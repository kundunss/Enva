using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class Domain
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Domain name is required")]
        [Display(Name = "Domain Name")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Company")]
        public int? CompanyId { get; set; }
        
        // Navigation properties
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
        public ICollection<MailEntry>? MailEntries { get; set; }
    }
}