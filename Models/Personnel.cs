using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class Personnel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
        
        [Required(ErrorMessage = "Site is required")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        
        [ForeignKey("SiteId")]
        public Site? Site { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<MailEntry>? MailEntries { get; set; }
        public ICollection<InventoryItem>? AssignedInventoryItems { get; set; }
        public ICollection<InventoryAssignmentHistory>? InventoryAssignmentHistory { get; set; }
    }
}