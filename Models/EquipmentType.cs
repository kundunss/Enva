using System.ComponentModel.DataAnnotations;

namespace SistemApp.Models
{
    public class EquipmentType
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Equipment Type")]
        public string Name { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<SystemHardware>? SystemHardware { get; set; }
        public ICollection<InventoryItem>? InventoryItems { get; set; }
    }
}