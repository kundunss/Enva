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
    public class EnvaPayController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EnvaPayController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? companyId, DateTime? startDate, DateTime? endDate)
        {
            var companies = await _context.Companies.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");

            var query = _context.PayEntries.Include(p => p.Company).AsQueryable();
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(p => p.CompanyId == companyId.Value);
            }
            if (startDate.HasValue)
            {
                query = query.Where(p => p.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.Date <= endDate.Value);
            }
            var model = await query
                .OrderBy(p => p.Company.Name)
                .ThenBy(p => p.Date)
                .ToListAsync();

            // Borç hesaplama: Seçili firma için hizmet toplamı - ödeme toplamı
            double? currentDebt = null;
            if (companyId.HasValue && companyId.Value > 0)
            {
                var serviceTotal = await _context.ServiceEntries.Where(s => s.CompanyId == companyId.Value).SumAsync(s => (double)s.Amount);
                var payTotal = await _context.PayEntries.Where(p => p.CompanyId == companyId.Value).SumAsync(p => (double)p.Amount);
                currentDebt = serviceTotal - payTotal;
            }
            ViewBag.CurrentDebt = currentDebt;

            ViewBag.SelectedCompanyId = companyId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(model);
        }

        // GET: EnvaPay/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: EnvaPay/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Date,Description,Amount")] PayEntry entry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entry);
                await _context.SaveChangesAsync();
                // Kayıttan sonra borç ViewBag'ini güncellemek için ilgili firmaya yönlendirme
                return RedirectToAction(nameof(Index), new { companyId = entry.CompanyId });
            }
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }

        // GET: EnvaPay/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var entry = await _context.PayEntries.FindAsync(id);
            if (entry == null)
                return NotFound();
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }

        // POST: EnvaPay/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Date,Description,Amount")] PayEntry entry)
        {
            if (id != entry.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayEntryExists(entry.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", entry.CompanyId);
            return View(entry);
        }

        // GET: EnvaPay/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var entry = await _context.PayEntries.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
            if (entry == null)
                return NotFound();
            return View(entry);
        }

        // POST: EnvaPay/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.PayEntries.FindAsync(id);
            if (entry != null)
            {
                _context.PayEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PayEntryExists(int id)
        {
            return _context.PayEntries.Any(e => e.Id == id);
        }
    }
}
