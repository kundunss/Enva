using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemApp.Controllers
{
    [Authorize]
    public class DomainsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DomainsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Domains
        public async Task<IActionResult> Index()
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Seçili firmaya ait domainleri getir
            var domains = await _context.Domains
                .Where(d => d.CompanyId == selectedCompanyId.Value)
                .OrderBy(d => d.Name)
                .ToListAsync();

            // Seçili firma adını ViewBag'e ekle
            var selectedCompany = await _context.Companies.FindAsync(selectedCompanyId.Value);
            ViewBag.SelectedCompanyName = selectedCompany?.Name;

            return View(domains);
        }

        // GET: Domains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domain == null)
            {
                return NotFound();
            }

            return View(domain);
        }

        // GET: Domains/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View();
        }

        // POST: Domains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Domain domain)
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Domain'i seçili firmaya ata
                domain.CompanyId = selectedCompanyId.Value;
                _context.Add(domain);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(domain);
        }

        // GET: Domains/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
            {
                return NotFound();
            }
            return View(domain);
        }

        // POST: Domains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Domain domain)
        {
            if (id != domain.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(domain);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DomainExists(domain.Id))
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
            return View(domain);
        }

        // GET: Domains/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domain == null)
            {
                return NotFound();
            }

            return View(domain);
        }

        // POST: Domains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if domain is in use
            bool isInUse = await _context.MailEntries.AnyAsync(m => m.DomainId == id);

            if (isInUse)
            {
                ModelState.AddModelError(string.Empty, "This domain cannot be deleted because it is in use in Mail List.");
                var domain = await _context.Domains.FindAsync(id);
                return View(domain);
            }

            var domainToDelete = await _context.Domains.FindAsync(id);
            if (domainToDelete != null)
            {
                _context.Domains.Remove(domainToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DomainExists(int id)
        {
            return _context.Domains.Any(e => e.Id == id);
        }
    }
}