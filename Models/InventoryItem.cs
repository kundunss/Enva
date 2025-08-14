using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Equipment type is required")]
        [Display(Name = "Equipment Type")]
        public int EquipmentTypeId { get; set; }
        
        [ForeignKey("EquipmentTypeId")]
        public EquipmentType? EquipmentType { get; set; }
        
        [Required(ErrorMessage = "Seri numarası gereklidir")]
        [Display(Name = "Seri Numarası")]
        [StringLength(50, ErrorMessage = "Seri numarası en fazla 50 karakter olabilir")]
        public string SerialNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Personnel is required")]
        [Display(Name = "Assigned To")]
        public int PersonnelId { get; set; }
        
        [ForeignKey("PersonnelId")]
        public Personnel? Personnel { get; set; }
        
        [Display(Name = "Is Archived")]
        public bool IsArchived { get; set; } = false;
        
        // Site reference through Personnel
        [NotMapped]
        public int SiteId => Personnel?.SiteId ?? 0;
        
        [NotMapped]
        public string SiteName => Personnel?.Site?.Name ?? string.Empty;
        
        // Navigation properties
        public ICollection<InventoryAssignmentHistory>? AssignmentHistory { get; set; }

	// QR kod URL'si
	[NotMapped]
	public string QrCodeUrl { get; set; } = string.Empty;
    }
}