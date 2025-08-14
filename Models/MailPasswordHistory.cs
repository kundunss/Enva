using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class MailPasswordHistory
    {
        public int Id { get; set; }
        
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        
        [Required]
        public DateTime ChangedDate { get; set; } = DateTime.Now;
        
        [Required]
        public int MailEntryId { get; set; }
        
        [ForeignKey("MailEntryId")]
        public MailEntry? MailEntry { get; set; }
    }
}