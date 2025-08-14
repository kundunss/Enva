using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class PasswordHistory
    {
        public int Id { get; set; }
        
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        
        [Required]
        public DateTime ChangedDate { get; set; } = DateTime.Now;
        
        [Required]
        public int SystemHardwareId { get; set; }
        
        [ForeignKey("SystemHardwareId")]
        public SystemHardware? SystemHardware { get; set; }
    }
}