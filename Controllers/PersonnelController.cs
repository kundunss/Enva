using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Text;

namespace SistemApp.Controllers
{
    [Authorize]
    public class PersonnelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonnelController(ApplicationDbContext context)
        {
            _context = context;
            // EPPlus lisans ayarı - NonCommercial kullanım için
            ExcelPackage.License.SetNonCommercialPersonal("Enva");
        }

        // GET: Personnel
        public async Task<IActionResult> Index(string searchString, int? siteId, bool showActiveOnly = true)
        {
            // Session'dan seçilen firmayı al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Şantiyeleri ViewBag'e ekle
            ViewBag.Sites = await _context.Sites
                .Where(s => s.CompanyId == selectedCompanyId.Value)
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Filtreleme parametrelerini ViewBag'e ekle
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSiteId = siteId;
            ViewBag.ShowActiveOnly = showActiveOnly;

            var personnel = _context.Personnel
                .Include(p => p.Site)
                .Where(p => p.Site.CompanyId == selectedCompanyId.Value);

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
            {
                personnel = personnel.Where(p => p.FirstName.Contains(searchString) || 
                                                p.LastName.Contains(searchString) ||
                                                p.Site.Name.Contains(searchString));
            }

            // Şantiye filtresi
            if (siteId.HasValue)
            {
                personnel = personnel.Where(p => p.SiteId == siteId.Value);
            }

            // Aktif çalışan filtresi
            if (showActiveOnly)
            {
                personnel = personnel.Where(p => p.IsActive);
            }

            var result = await personnel
                .OrderBy(p => p.Site.Name)
                .ThenBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();

            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            return View(result);
        }

        // GET: Personnel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personnel = await _context.Personnel
                .Include(p => p.Site)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personnel == null)
            {
                return NotFound();
            }

            return View(personnel);
        }

        // GET: Personnel/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sadece seçili firmaya ait şantiyeleri listele
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name");
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View();
        }

        // POST: Personnel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,SiteId,IsActive")] Personnel personnel)
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Add(personnel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Sadece seçili firmaya ait şantiyeleri listele
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", personnel.SiteId);
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(personnel);
        }

        // GET: Personnel/Edit/5
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

            var personnel = await _context.Personnel.FindAsync(id);
            if (personnel == null)
            {
                return NotFound();
            }
            
            // Sadece seçili firmaya ait şantiyeleri listele
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", personnel.SiteId);
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(personnel);
        }

        // POST: Personnel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,SiteId,IsActive")] Personnel personnel)
        {
            if (id != personnel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current state of the personnel to check if IsActive changed
                    var currentPersonnel = await _context.Personnel.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    bool statusChanged = currentPersonnel != null && currentPersonnel.IsActive != personnel.IsActive;
                    
                    _context.Update(personnel);
                    await _context.SaveChangesAsync();
                    
                    // If personnel was marked as inactive, archive their mail entries
                    if (statusChanged && !personnel.IsActive)
                    {
                        var mailEntries = await _context.MailEntries.Where(m => m.PersonnelId == id && !m.IsArchived).ToListAsync();
                        foreach (var entry in mailEntries)
                        {
                            entry.IsArchived = true;
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonnelExists(personnel.Id))
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
            
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Sadece seçili firmaya ait şantiyeleri listele
            ViewData["SiteId"] = new SelectList(_context.Sites.Where(s => s.CompanyId == selectedCompanyId.Value).OrderBy(s => s.Name), "Id", "Name", personnel.SiteId);
            
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(personnel);
        }

        // GET: Personnel/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personnel = await _context.Personnel
                .Include(p => p.Site)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personnel == null)
            {
                return NotFound();
            }

            return View(personnel);
        }

        // POST: Personnel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if personnel is in use
            bool isInUse = await _context.MailEntries.AnyAsync(m => m.PersonnelId == id) ||
                           await _context.InventoryItems.AnyAsync(i => i.PersonnelId == id);

            if (isInUse)
            {
                ModelState.AddModelError(string.Empty, "This personnel cannot be deleted because they are referenced in Mail List or Inventory.");
                var personnel = await _context.Personnel.Include(p => p.Site).FirstOrDefaultAsync(p => p.Id == id);
                return View(personnel);
            }

            var personnelToDelete = await _context.Personnel.FindAsync(id);
            if (personnelToDelete != null)
            {
                _context.Personnel.Remove(personnelToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PersonnelExists(int id)
        {
            return _context.Personnel.Any(e => e.Id == id);
        }

        // GET: Personnel/Import
        [Authorize(Roles = "Admin")]
        public IActionResult Import()
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            return View();
        }

        // POST: Personnel/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir Excel dosyası seçin.");
                ViewBag.SelectedCompanyId = selectedCompanyId.Value;
                ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
                return View();
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                ModelState.AddModelError("", "Sadece Excel dosyaları (.xlsx, .xls) kabul edilir.");
                ViewBag.SelectedCompanyId = selectedCompanyId.Value;
                ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
                return View();
            }

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("My Name");
                
                var sites = await _context.Sites
                    .Where(s => s.CompanyId == selectedCompanyId.Value)
                    .ToListAsync();

                var importedCount = 0;
                var errorMessages = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        for (int row = 2; row <= rowCount; row++) // Skip header row
                        {
                            try
                            {
                                var firstName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                var lastName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                var siteName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                var isActiveStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(siteName))
                                {
                                    errorMessages.Add($"Satır {row}: Ad, Soyad ve Şantiye alanları zorunludur.");
                                    continue;
                                }

                                var site = sites.FirstOrDefault(s => s.Name.Equals(siteName, StringComparison.OrdinalIgnoreCase));
                                if (site == null)
                                {
                                    errorMessages.Add($"Satır {row}: '{siteName}' şantiyesi bulunamadı.");
                                    continue;
                                }

                                bool isActive = true;
                                if (!string.IsNullOrEmpty(isActiveStr))
                                {
                                    if (isActiveStr.Equals("Aktif", StringComparison.OrdinalIgnoreCase) || 
                                        isActiveStr.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                                        isActiveStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                        isActiveStr.Equals("True", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isActive = true;
                                    }
                                    else if (isActiveStr.Equals("Pasif", StringComparison.OrdinalIgnoreCase) || 
                                             isActiveStr.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
                                             isActiveStr.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                                             isActiveStr.Equals("False", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isActive = false;
                                    }
                                }

                                // Check if personnel already exists
                                var existingPersonnel = await _context.Personnel
                                    .Include(p => p.Site)
                                    .FirstOrDefaultAsync(p => p.FirstName == firstName && 
                                                            p.LastName == lastName && 
                                                            p.Site.CompanyId == selectedCompanyId.Value);

                                if (existingPersonnel != null)
                                {
                                    errorMessages.Add($"Satır {row}: {firstName} {lastName} personeli zaten mevcut.");
                                    continue;
                                }

                                var personnel = new Personnel
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    SiteId = site.Id,
                                    IsActive = isActive
                                };

                                _context.Personnel.Add(personnel);
                                importedCount++;
                            }
                            catch (Exception ex)
                            {
                                errorMessages.Add($"Satır {row}: {ex.Message}");
                            }
                        }
                    }
                }

                if (importedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                if (errorMessages.Any())
                {
                    ViewBag.ErrorMessages = errorMessages;
                    ViewBag.ImportedCount = importedCount;
                }
                else
                {
                    TempData["SuccessMessage"] = $"{importedCount} personel başarıyla içe aktarıldı.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Dosya işlenirken hata oluştu: {ex.Message}");
            }

            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            return View();
        }

        // GET: Personnel/ExportTemplate
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportTemplate()
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("My Name");
                
                var sites = await _context.Sites
                    .Where(s => s.CompanyId == selectedCompanyId.Value)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Personel Şablonu");

                    // Header row
                    worksheet.Cells[1, 1].Value = "Ad";
                    worksheet.Cells[1, 2].Value = "Soyad";
                    worksheet.Cells[1, 3].Value = "Şantiye";
                    worksheet.Cells[1, 4].Value = "Durum";

                    // Style header row
                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    // Example data
                    worksheet.Cells[2, 1].Value = "Ahmet";
                    worksheet.Cells[2, 2].Value = "Yılmaz";
                    worksheet.Cells[2, 3].Value = sites.FirstOrDefault()?.Name ?? "Örnek Şantiye";
                    worksheet.Cells[2, 4].Value = "Aktif";

                    worksheet.Cells[3, 1].Value = "Mehmet";
                    worksheet.Cells[3, 2].Value = "Demir";
                    worksheet.Cells[3, 3].Value = sites.FirstOrDefault()?.Name ?? "Örnek Şantiye";
                    worksheet.Cells[3, 4].Value = "Pasif";

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Add instructions sheet
                    var instructionsSheet = package.Workbook.Worksheets.Add("Talimatlar");
                    instructionsSheet.Cells[1, 1].Value = "PERSONEL İÇE AKTARMA TALİMATLARI";
                    instructionsSheet.Cells[1, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[1, 1].Style.Font.Size = 14;

                    instructionsSheet.Cells[3, 1].Value = "1. Ad: Personelin adı (Zorunlu)";
                    instructionsSheet.Cells[4, 1].Value = "2. Soyad: Personelin soyadı (Zorunlu)";
                    instructionsSheet.Cells[5, 1].Value = "3. Şantiye: Personelin çalışacağı şantiye adı (Zorunlu)";
                    instructionsSheet.Cells[6, 1].Value = "4. Durum: Aktif/Pasif (İsteğe bağlı, varsayılan: Aktif)";

                    instructionsSheet.Cells[8, 1].Value = "Geçerli Şantiyeler:";
                    instructionsSheet.Cells[8, 1].Style.Font.Bold = true;
                    
                    int row = 9;
                    foreach (var site in sites)
                    {
                        instructionsSheet.Cells[row, 1].Value = $"- {site.Name}";
                        row++;
                    }

                    instructionsSheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var companyName = HttpContext.Session.GetString("SelectedCompanyName") ?? "Firma";
                    var fileName = $"Personel_Sablonu_{companyName}_{DateTime.Now:yyyyMMdd}.xlsx";

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Şablon oluşturulurken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}