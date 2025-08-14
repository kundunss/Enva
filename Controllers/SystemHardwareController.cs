using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Text;

namespace SistemApp.Controllers
{
    [Authorize]
    public class SystemHardwareController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SystemHardwareController(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.License.SetNonCommercialPersonal("Enva");
        }

        // GET: SystemHardware
        public async Task<IActionResult> Index(int? siteId)
        {
            // Session'dan seçilen firmayı al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.SystemHardware
                .Include(s => s.EquipmentType)
                .Include(s => s.Site)
                .Include(s => s.Company)
                .Where(s => s.CompanyId == selectedCompanyId.Value)
                .AsQueryable();

            if (siteId.HasValue)
            {
                query = query.Where(s => s.SiteId == siteId.Value);
            }

            var systemHardware = await query
                .OrderBy(s => s.Site.Name)
                .ThenBy(s => s.EquipmentType.Name)
                .ToListAsync();
            
            // Sadece seçilen firmaya ait siteleri göster
            ViewBag.Sites = await _context.Sites
                .Where(s => s.CompanyId == selectedCompanyId.Value)
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            ViewBag.SelectedSiteId = siteId;
            return View(systemHardware);
        }

        // GET: SystemHardware/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemHardware = await _context.SystemHardware
                .Include(s => s.EquipmentType)
                .Include(s => s.Site)
                .Include(s => s.PasswordHistory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (systemHardware == null)
            {
                return NotFound();
            }

            return View(systemHardware);
        }

        // GET: SystemHardware/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name");
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name");
            
            // Seçili firma bilgilerini ViewBag'e ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View();
        }

        // POST: SystemHardware/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,EquipmentTypeId,Description,IpAddress,Username,Password,SiteId,CompanyId")] SystemHardware systemHardware)
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // CompanyId'yi session'dan al
            systemHardware.CompanyId = selectedCompanyId.Value;

            if (ModelState.IsValid)
            {
                _context.Add(systemHardware);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name", systemHardware.EquipmentTypeId);
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", systemHardware.SiteId);
            
            // Seçili firma bilgilerini ViewBag'e ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(systemHardware);
        }

        // GET: SystemHardware/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var systemHardware = await _context.SystemHardware.FindAsync(id);
            if (systemHardware == null)
            {
                return NotFound();
            }
            
            ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name", systemHardware.EquipmentTypeId);
            
            // Sadece seçili firmaya ait şantiyeleri listele
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", systemHardware.SiteId);
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(systemHardware);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int id, [Bind("Id,EquipmentTypeId,Description,IpAddress,Username,Password,SiteId,CompanyId")] SystemHardware systemHardware)
{
    if (id != systemHardware.Id)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            // Mevcut kaydı veritabanından çek
            var currentHardware = await _context.SystemHardware.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
            if (currentHardware == null) return NotFound();

            // Şifre değiştiyse ve boş değilse password history ekle
            if (!string.IsNullOrEmpty(systemHardware.Password) && systemHardware.Password != currentHardware.Password)
            {
                _context.PasswordHistory.Add(new PasswordHistory
                {
                    SystemHardwareId = id,
                    OldPassword = currentHardware.Password,
                    ChangedDate = DateTime.Now
                });

                currentHardware.Password = systemHardware.Password; // Şifreyi güncelle
            }

            // Diğer alanları güncelle
            currentHardware.EquipmentTypeId = systemHardware.EquipmentTypeId;
            currentHardware.Description = systemHardware.Description;
            currentHardware.IpAddress = systemHardware.IpAddress;
            currentHardware.Username = systemHardware.Username;
            currentHardware.SiteId = systemHardware.SiteId;
            currentHardware.CompanyId = systemHardware.CompanyId;

            _context.Update(currentHardware);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SystemHardwareExists(systemHardware.Id))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction(nameof(Index));
    }

    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    if (selectedCompanyId == null)
        return RedirectToAction("Login", "Account");

    ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name", systemHardware.EquipmentTypeId);
    ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", systemHardware.SiteId);
    
    ViewBag.SelectedCompanyId = selectedCompanyId.Value;
    ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

    return View(systemHardware);
}

        // GET: SystemHardware/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemHardware = await _context.SystemHardware
                .Include(s => s.EquipmentType)
                .Include(s => s.Site)
                .Include(s => s.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (systemHardware == null)
            {
                return NotFound();
            }

            return View(systemHardware);
        }

        // POST: SystemHardware/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var systemHardware = await _context.SystemHardware.FindAsync(id);
            if (systemHardware != null)
            {
                // Delete related password history first
                var passwordHistory = await _context.PasswordHistory.Where(p => p.SystemHardwareId == id).ToListAsync();
                _context.PasswordHistory.RemoveRange(passwordHistory);
                
                // Then delete the system hardware
                _context.SystemHardware.Remove(systemHardware);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SystemHardwareExists(int id)
        {
            return _context.SystemHardware.Any(e => e.Id == id);
        }

        // API endpoint to get sites by company
        [HttpGet]
        public async Task<IActionResult> GetSitesByCompany(int companyId)
        {
            var sites = await _context.Sites
                .Where(s => s.CompanyId == companyId)
                .OrderBy(s => s.Name)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToListAsync();
            
            return Json(sites);
        }

        [Authorize]
public async Task<IActionResult> ExportToExcel(int? siteId)
{
    // Oturumdan doğru anahtarı kullanarak firma ID'sini al
    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    
    if (!selectedCompanyId.HasValue)
    {
        return RedirectToAction("Login", "Account");
    }

    var query = _context.SystemHardware
        .Include(s => s.EquipmentType)
        .Include(s => s.Site)
        .Where(s => s.CompanyId == selectedCompanyId.Value); // Sorguyu da güncelliyoruz

    if (siteId.HasValue)
    {
        query = query.Where(s => s.SiteId == siteId.Value);
    }

    var systemHardware = await query.ToListAsync();

    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add("Sistem Donanımları");

        // Başlıklar
        worksheet.Cells[1, 1].Value = "Sıra No";
        worksheet.Cells[1, 2].Value = "Şantiye";
        worksheet.Cells[1, 3].Value = "Donanım Türü";
        worksheet.Cells[1, 4].Value = "Açıklama";
        worksheet.Cells[1, 5].Value = "Kullanıcı Adı";
        worksheet.Cells[1, 6].Value = "Şifre";
        
        // Başlık stilini ayarla
        using (var range = worksheet.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Verileri ekle
        for (int i = 0; i < systemHardware.Count; i++)
        {
            var item = systemHardware[i];
            int row = i + 2;

            worksheet.Cells[row, 1].Value = i + 1;
            worksheet.Cells[row, 2].Value = item.Site?.Name;
            worksheet.Cells[row, 3].Value = item.EquipmentType?.Name;
            worksheet.Cells[row, 4].Value = item.Description;
            worksheet.Cells[row, 5].Value = item.Username;
            worksheet.Cells[row, 6].Value = item.Password;
        }

        // Sütun genişliklerini ayarla
        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"SistemDonanımlari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
    }
}