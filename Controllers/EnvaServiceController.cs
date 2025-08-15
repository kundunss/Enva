using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemApp.Controllers
{
    [Authorize]
    public class EnvaServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnvaServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? companyId, DateTime? startDate, DateTime? endDate)
        {
            var companies = await _context.Companies.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");

            var query = _context.ServiceEntries.Include(s => s.Company).AsQueryable();
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(s => s.CompanyId == companyId.Value);
            }
            if (startDate.HasValue)
            {
                query = query.Where(s => s.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(s => s.Date <= endDate.Value);
            }
            var model = await query
                .OrderBy(s => s.Company.Name)
                .ThenBy(s => s.Date)
                .ToListAsync();
            ViewBag.SelectedCompanyId = companyId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(model);
        }

        // GET: EnvaService/Create
        public async Task<IActionResult> Create()
        {
            // Kullanıcının seçebileceği şirket listesini ViewData'ya atıyoruz.
            // Bu şekilde View'de asp-for ile CompanyId'yi bind edebilirsiniz.
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: EnvaService/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind attribute'u, model bağlamayı daha güvenli hale getirir.
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Date,Description,Amount")] ServiceEntry entry)
        {

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == entry.CompanyId);
            if (!companyExists)
            {
                ModelState.AddModelError("CompanyId", "Seçilen firma bulunamadı.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Model geçerli değilse, View'a dönmeden önce listeyi tekrar yükleyin
            // böylece sayfada hata mesajları gösterilebilir ve dropdown boş kalmaz.
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }

        // GET: EnvaService/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.ServiceEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }
            
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }

        // POST: EnvaService/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Date,Description,Amount")] ServiceEntry entry)
        {

            if (id != entry.Id)
            {
                return NotFound();
            }

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == entry.CompanyId);
            if (!companyExists)
            {
                ModelState.AddModelError("CompanyId", "Seçilen firma bulunamadı.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceEntryExists(entry.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Firma bilgisini tekrar doldur
            entry.Company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == entry.CompanyId);
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }
        
        // ... (Diğer metotlar aynı kalabilir) ...
        
        private bool ServiceEntryExists(int id)
        {
            return _context.ServiceEntries.Any(e => e.Id == id);
        }
    }
}
