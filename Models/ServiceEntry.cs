using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemApp.Models
{
    public class ServiceEntry
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Amount { get; set; }
    }
}
