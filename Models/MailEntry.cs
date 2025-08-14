using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class MailEntry
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Personnel is required")]
        [Display(Name = "Personnel")]
        public int PersonnelId { get; set; }
        
        [ForeignKey("PersonnelId")]
        public Personnel? Personnel { get; set; }
        
        [Required(ErrorMessage = "Email username is required")]
        [Display(Name = "Email Username")]
        public string EmailUsername { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Domain is required")]
        [Display(Name = "Domain")]
        public int DomainId { get; set; }
        
        [ForeignKey("DomainId")]
        public Domain? Domain { get; set; }
        
        [Display(Name = "Password")]
	public string? Password { get; set; } // nullable
        
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } = string.Empty;
        
        [Display(Name = "Is Archived")]
        public bool IsArchived { get; set; } = false;
        
        // Navigation properties
        public ICollection<MailPasswordHistory>? PasswordHistory { get; set; }
        
        // Site reference through Personnel
        [NotMapped]
        public int SiteId => Personnel?.SiteId ?? 0;
        
        [NotMapped]
        public string SiteName => Personnel?.Site?.Name ?? string.Empty;
    }
}