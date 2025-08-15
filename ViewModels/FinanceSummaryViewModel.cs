using SistemApp.Models;
using System.Collections.Generic;

namespace SistemApp.ViewModels
{
    public class FinanceSummaryViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal TotalKazanc { get; set; }
        public decimal TotalAlacak { get; set; }
        public decimal TotalTahsilat { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public DateTime? LastPayDate { get; set; }
        public List<ServiceEntry> ServiceEntries { get; set; }
        public List<PayEntry> PayEntries { get; set; }
    }
}
