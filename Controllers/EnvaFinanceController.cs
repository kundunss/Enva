using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SistemApp.Controllers
{
    public class EnvaFinanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EnvaFinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Tüm şirketleri, hizmet ve ödeme hareketleriyle birlikte çek
            var companies = _context.Companies
                .Include(c => c.ServiceEntries)
                .Include(c => c.PayEntries)
                .ToList();

            // Toplamlar
            var allServices = companies.SelectMany(c => c.ServiceEntries ?? new List<Models.ServiceEntry>()).ToList();
            var allPays = companies.SelectMany(c => c.PayEntries ?? new List<Models.PayEntry>()).ToList();

            decimal totalAlacak = allServices.Sum(s => s.Amount);
            decimal totalTahsilat = allPays.Sum(p => p.Amount);
            decimal kalanAlacak = totalAlacak - totalTahsilat;
            decimal tahsilatOrani = totalAlacak > 0 ? (totalTahsilat / totalAlacak) : 1m;

            // Bu ay ve yıl kazanç
            var now = DateTime.Now;
            decimal ayKazanc = allServices.Where(s => s.Date.Year == now.Year && s.Date.Month == now.Month).Sum(s => s.Amount);
            decimal yilKazanc = allServices.Where(s => s.Date.Year == now.Year).Sum(s => s.Amount);

            ViewBag.TotalAlacak = totalAlacak;
            ViewBag.TotalTahsilat = totalTahsilat;
            ViewBag.KalanAlacak = kalanAlacak;
            ViewBag.TahsilatOrani = tahsilatOrani;
            ViewBag.AyKazanc = ayKazanc;
            ViewBag.YilKazanc = yilKazanc;

            // Müşteri finansal durumu
            var musteriList = companies.Select(c => new {
                Musteri = c.Name,
                ToplamAlacak = c.ServiceEntries?.Sum(s => s.Amount) ?? 0m,
                ToplamTahsilat = c.PayEntries?.Sum(p => p.Amount) ?? 0m,
                KalanAlacak = (c.ServiceEntries?.Sum(s => s.Amount) ?? 0m) - (c.PayEntries?.Sum(p => p.Amount) ?? 0m),
                SonHizmet = c.ServiceEntries?.OrderByDescending(s => s.Date).FirstOrDefault()?.Date ?? DateTime.MinValue,
                SonOdeme = c.PayEntries?.OrderByDescending(p => p.Date).FirstOrDefault()?.Date
            }).ToList();
            ViewBag.MusteriFinans = musteriList;

            // Aylık kazanç trendi (sadece gerçek veriler)
            var aylar = allServices.Select(s => new { s.Date.Year, s.Date.Month })
                .Union(allPays.Select(p => new { p.Date.Year, p.Date.Month }))
                .Distinct()
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .Take(12)
                .ToList();

            var aylikTrend = aylar.Select(a => new {
                Ay = new DateTime(a.Year, a.Month, 1).ToString("MMMM yyyy", new CultureInfo("tr-TR")),
                Kazanc = allServices.Where(s => s.Date.Year == a.Year && s.Date.Month == a.Month).Sum(s => s.Amount),
                Tahsilat = allPays.Where(p => p.Date.Year == a.Year && p.Date.Month == a.Month).Sum(p => p.Amount),
            }).Select(x => new {
                x.Ay,
                x.Kazanc,
                x.Tahsilat,
                Fark = x.Tahsilat - x.Kazanc
            }).ToList();
            ViewBag.AylikTrend = aylikTrend;

            return View();
        }
    }
}
