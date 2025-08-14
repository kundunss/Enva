using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class InventoryAssignmentHistory
    {
        public int Id { get; set; }
        
        [Required]
        public int InventoryItemId { get; set; }
        
        [ForeignKey("InventoryItemId")]
        public InventoryItem? InventoryItem { get; set; }
        
        [Required]
        public int PersonnelId { get; set; }
        
        [ForeignKey("PersonnelId")]
        public Personnel? Personnel { get; set; }
        
        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}