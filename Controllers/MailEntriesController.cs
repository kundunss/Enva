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
    public class MailEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MailEntriesController(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.License.SetNonCommercialPersonal("Enva");
        }

        // GET: MailEntries
        public async Task<IActionResult> Index(bool showArchived = false, int? siteId = null)
        {
            // Session'dan seçilen firmayı al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<MailEntry> query = _context.MailEntries
                .Include(m => m.Domain)
                .Include(m => m.Personnel)
                .Include(m => m.Personnel.Site)
                .Where(m => m.Personnel.Site.CompanyId == selectedCompanyId.Value);

            if (!showArchived)
            {
                query = query.Where(m => !m.IsArchived);
            }

            if (siteId.HasValue)
            {
                query = query.Where(m => m.Personnel.SiteId == siteId.Value);
            }
		// Santiyelere gore aktif ve pasif mail sayilarini grupla
var siteMailCountsQuery = _context.MailEntries
    .Include(m => m.Personnel)
    .ThenInclude(p => p.Site)
    .Where(m => m.Personnel.Site.CompanyId == selectedCompanyId.Value);

// Eğer arşivli mailleri göstermiyorsak, onları filtrele
if (!showArchived)
{
    siteMailCountsQuery = siteMailCountsQuery.Where(m => !m.IsArchived);
}

// Şantiyelere göre gruplandır
var siteMailCounts = await siteMailCountsQuery
    .GroupBy(m => new { SiteName = m.Personnel.Site.Name, IsArchived = m.IsArchived })
    .Select(g => new { SiteName = g.Key.SiteName, IsArchived = g.Key.IsArchived, Count = g.Count() })
    .OrderBy(x => x.SiteName)
    .ToListAsync();

// Bu listeyi ViewBag'e ekle
ViewBag.SiteMailCounts = siteMailCounts;

// Genel toplamları hesapla (tüm listeyi kullanarak)
var totalActiveMails = await _context.MailEntries
    .Where(m => m.Personnel.Site.CompanyId == selectedCompanyId.Value && !m.IsArchived)
    .CountAsync();

var totalArchivedMails = await _context.MailEntries
    .Where(m => m.Personnel.Site.CompanyId == selectedCompanyId.Value && m.IsArchived)
    .CountAsync();

// Genel toplamları da ViewBag'e ekle
ViewBag.TotalActiveMails = totalActiveMails;
ViewBag.TotalArchivedMails = totalArchivedMails;
            var mailEntries = await query
                .OrderBy(m => m.Personnel.Site != null ? m.Personnel.Site.Name : "")
                .ThenBy(m => m.Personnel != null ? m.Personnel.FirstName : "")
                .ToListAsync();

            ViewData["ShowArchived"] = showArchived;
            ViewBag.Sites = await _context.Sites
                .Where(s => s.CompanyId == selectedCompanyId.Value)
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.SelectedSiteId = siteId;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            return View(mailEntries);
        }

        // GET: MailEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mailEntry = await _context.MailEntries
                .Include(m => m.Domain)
                .Include(m => m.Personnel)
                .ThenInclude(p => p.Site)
                .Include(m => m.PasswordHistory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailEntry == null)
            {
                return NotFound();
            }

            return View(mailEntry);
        }

        // GET: MailEntries/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sadece seçili firmaya ait domainleri listele
            ViewData["DomainId"] = new SelectList(_context.Domains.Where(d => d.CompanyId == selectedCompanyId.Value).OrderBy(d => d.Name), "Id", "Name");
            
            // Sadece seçili firmaya ait personelleri listele
            ViewData["PersonnelId"] = new SelectList(
                _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
                    .Include(p => p.Site)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
                "Id", "FullInfo");
                
            // Seçili firma bilgilerini ViewBag'e ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View();
        }

        // POST: MailEntries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,PersonnelId,EmailUsername,DomainId,Password,IsArchived")] MailEntry mailEntry)
        {
            if (ModelState.IsValid)
            {
                // Generate the full email address
                var domain = await _context.Domains.FindAsync(mailEntry.DomainId);
                if (domain != null)
                {
                    mailEntry.EmailAddress = $"{mailEntry.EmailUsername}@{domain.Name}";
                }

                _context.Add(mailEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Session'dan seçili firma ID'sini al
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sadece seçili firmaya ait domainleri ve personelleri listele
            ViewData["DomainId"] = new SelectList(_context.Domains.Where(d => d.CompanyId == selectedCompanyId.Value).OrderBy(d => d.Name), "Id", "Name", mailEntry.DomainId);
            ViewData["PersonnelId"] = new SelectList(
                _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
                    .Include(p => p.Site)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
                "Id", "FullInfo", mailEntry.PersonnelId);
                
            // Seçili firma bilgilerini ViewBag'e ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            return View(mailEntry);
        }

        // GET: MailEntries/Edit/5
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

            var mailEntry = await _context.MailEntries.FindAsync(id);
            if (mailEntry == null)
            {
                return NotFound();
            }
            
            // Sadece seçili firmaya ait domainleri listele
            ViewData["DomainId"] = new SelectList(_context.Domains.Where(d => d.CompanyId == selectedCompanyId.Value).OrderBy(d => d.Name), "Id", "Name", mailEntry.DomainId);
            
            // Sadece seçili firmaya ait personelleri listele
            ViewData["PersonnelId"] = new SelectList(
                _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
                    .Include(p => p.Site)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
                "Id", "FullInfo", mailEntry.PersonnelId);
                
            // ViewBag'e seçili firma bilgilerini ekle
            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");
            
            return View(mailEntry);
        }

        // POST: MailEntries/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int id, [Bind("Id,PersonnelId,EmailUsername,DomainId,Password,IsArchived")] MailEntry mailEntry)
{
    if (id != mailEntry.Id)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            // Veritabanındaki mevcut mailEntry
            var currentMailEntry = await _context.MailEntries.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (currentMailEntry == null) return NotFound();

            // Eğer Password alanı doluysa güncelle, boşsa eski şifreyi koru
            if (!string.IsNullOrEmpty(mailEntry.Password) && mailEntry.Password != currentMailEntry.Password)
            {
                // Password değişmişse history'e ekle
                _context.MailPasswordHistory.Add(new MailPasswordHistory
                {
                    MailEntryId = id,
                    OldPassword = currentMailEntry.Password,
                    ChangedDate = DateTime.Now
                });
            }
            else
            {
                // Password boşsa veya değişmemişse eskiyi koru
                mailEntry.Password = currentMailEntry.Password;
            }

            // Email adresini domain ile oluştur
            var domain = await _context.Domains.FindAsync(mailEntry.DomainId);
            if (domain != null)
            {
                mailEntry.EmailAddress = $"{mailEntry.EmailUsername}@{domain.Name}";
            }

            _context.Update(mailEntry);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MailEntryExists(mailEntry.Id))
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

    // Formu tekrar doldur
    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    if (selectedCompanyId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    ViewData["DomainId"] = new SelectList(_context.Domains.Where(d => d.CompanyId == selectedCompanyId.Value).OrderBy(d => d.Name), "Id", "Name", mailEntry.DomainId);
    ViewData["PersonnelId"] = new SelectList(
        _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
            .Include(p => p.Site)
            .OrderBy(p => p.Site.Name)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
        "Id", "FullInfo", mailEntry.PersonnelId);

    ViewBag.SelectedCompanyId = selectedCompanyId.Value;
    ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

    return View(mailEntry);
}

        // GET: MailEntries/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mailEntry = await _context.MailEntries
                .Include(m => m.Domain)
                .Include(m => m.Personnel)
                .ThenInclude(p => p.Site)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailEntry == null)
            {
                return NotFound();
            }

            return View(mailEntry);
        }

        // POST: MailEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mailEntry = await _context.MailEntries.FindAsync(id);
            if (mailEntry != null)
            {
                // Delete related password history first
                var passwordHistory = await _context.MailPasswordHistory.Where(p => p.MailEntryId == id).ToListAsync();
                _context.MailPasswordHistory.RemoveRange(passwordHistory);
                
                // Then delete the mail entry
                _context.MailEntries.Remove(mailEntry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MailEntries/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int id)
        {
            var mailEntry = await _context.MailEntries.FindAsync(id);
            if (mailEntry != null)
            {
                mailEntry.IsArchived = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MailEntries/Unarchive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unarchive(int id)
        {
            var mailEntry = await _context.MailEntries.FindAsync(id);
            if (mailEntry != null)
            {
                mailEntry.IsArchived = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { showArchived = true });
        }

        private bool MailEntryExists(int id)
        {
            return _context.MailEntries.Any(e => e.Id == id);
        }

        // GET: MailEntries/Import
        [Authorize(Roles = "Admin")]
        public IActionResult Import()
        {
            return View();
        }

        // POST: MailEntries/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.ErrorMessage = "Lütfen bir Excel dosyası seçin.";
                return View();
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                ViewBag.ErrorMessage = "Lütfen geçerli bir Excel dosyası (.xlsx veya .xls) yükleyin.";
                return View();
            }

            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                var errors = new List<string>();
                var successCount = 0;

                // Get all personnel and domains for the selected company
                var personnel = await _context.Personnel
                    .Include(p => p.Site)
                    .Where(p => p.Site.CompanyId == selectedCompanyId.Value)
                    .ToListAsync();

                var domains = await _context.Domains
                    .Where(d => d.CompanyId == selectedCompanyId.Value)
                    .ToListAsync();

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var personnelName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var siteName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var emailUsername = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var domainName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var password = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(personnelName) || string.IsNullOrEmpty(siteName) ||
                            string.IsNullOrEmpty(emailUsername) || string.IsNullOrEmpty(domainName) ||
                            string.IsNullOrEmpty(password))
                        {
                            errors.Add($"Satır {row}: Tüm alanlar doldurulmalıdır.");
                            continue;
                        }

                        // Find personnel by name and site
                        var foundPersonnel = personnel.FirstOrDefault(p =>
                            p.FullName.Equals(personnelName, StringComparison.OrdinalIgnoreCase) &&
                            p.Site.Name.Equals(siteName, StringComparison.OrdinalIgnoreCase));

                        if (foundPersonnel == null)
                        {
                            errors.Add($"Satır {row}: '{personnelName}' adlı personel '{siteName}' sitesinde bulunamadı.");
                            continue;
                        }

                        // Find domain
                        var foundDomain = domains.FirstOrDefault(d =>
                            d.Name.Equals(domainName, StringComparison.OrdinalIgnoreCase));

                        if (foundDomain == null)
                        {
                            errors.Add($"Satır {row}: '{domainName}' domain'i bulunamadı.");
                            continue;
                        }

                        // Check if mail entry already exists
                       var existingMailEntry = await _context.MailEntries
    .FirstOrDefaultAsync(m =>
        m.PersonnelId == foundPersonnel.Id &&
        m.EmailUsername.ToLower() == emailUsername.ToLower() &&
        m.DomainId == foundDomain.Id
    );

                        if (existingMailEntry != null)
                        {
                            errors.Add($"Satır {row}: Bu e-posta adresi zaten mevcut: {emailUsername}@{domainName}");
                            continue;
                        }

                        // Create new mail entry
                        var mailEntry = new MailEntry
                        {
                            PersonnelId = foundPersonnel.Id,
                            EmailUsername = emailUsername,
                            DomainId = foundDomain.Id,
                            Password = password,
                            EmailAddress = $"{emailUsername}@{domainName}",
                            IsArchived = !foundPersonnel.IsActive // Pasif personel için arşivli olarak kaydet
                        };

                        _context.MailEntries.Add(mailEntry);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Satır {row}: Hata - {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                if (errors.Any())
                {
                    ViewBag.ErrorMessage = string.Join("\n", errors);
                }

                if (successCount > 0)
                {
                    ViewBag.SuccessMessage = $"{successCount} e-posta kaydı başarıyla eklendi.";
                }

                if (!errors.Any() && successCount > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Dosya işlenirken hata oluştu: {ex.Message}";
            }

            return View();
        }

        // GET: MailEntries/ExportTemplate
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportTemplate()
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("E-posta Kayıtları");

                // Headers
                worksheet.Cells[1, 1].Value = "Personel Adı";
                worksheet.Cells[1, 2].Value = "Site";
                worksheet.Cells[1, 3].Value = "E-posta Kullanıcı Adı";
                worksheet.Cells[1, 4].Value = "Domain";
                worksheet.Cells[1, 5].Value = "Şifre";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Example data
                worksheet.Cells[2, 1].Value = "Ahmet Yılmaz";
                worksheet.Cells[2, 2].Value = "Merkez";
                worksheet.Cells[2, 3].Value = "ahmet.yilmaz";
                worksheet.Cells[2, 4].Value = "sirket.com";
                worksheet.Cells[2, 5].Value = "Sifre123!";

                worksheet.Cells[3, 1].Value = "Ayşe Kaya";
                worksheet.Cells[3, 2].Value = "Şube 1";
                worksheet.Cells[3, 3].Value = "ayse.kaya";
                worksheet.Cells[3, 4].Value = "sirket.com";
                worksheet.Cells[3, 5].Value = "Sifre456!";

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add instructions
                var instructionSheet = package.Workbook.Worksheets.Add("Talimatlar");
                instructionSheet.Cells[1, 1].Value = "E-POSTA KAYITLARI İÇE AKTARMA TALİMATLARI";
                instructionSheet.Cells[1, 1].Style.Font.Bold = true;
                instructionSheet.Cells[1, 1].Style.Font.Size = 14;

                instructionSheet.Cells[3, 1].Value = "1. Excel dosyasının ilk satırı başlık satırıdır, değiştirmeyin.";
                instructionSheet.Cells[4, 1].Value = "2. Personel Adı: Sistemde kayıtlı personelin tam adını yazın (Örn: Ahmet Yılmaz)";
                instructionSheet.Cells[5, 1].Value = "3. Site: Personelin çalıştığı sitenin adını yazın";
                instructionSheet.Cells[6, 1].Value = "4. E-posta Kullanıcı Adı: @ işaretinden önceki kısmı yazın (Örn: ahmet.yilmaz)";
                instructionSheet.Cells[7, 1].Value = "5. Domain: E-posta domain'ini yazın (Örn: sirket.com)";
                instructionSheet.Cells[8, 1].Value = "6. Şifre: E-posta hesabının şifresini yazın";
                instructionSheet.Cells[9, 1].Value = "7. Tüm alanlar zorunludur.";
                instructionSheet.Cells[10, 1].Value = "8. Aynı e-posta adresi birden fazla kez eklenemez.";
                instructionSheet.Cells[11, 1].Value = "9. Pasif personel için eklenen mail adresleri otomatik olarak arşivli olarak kaydedilir.";

                // Get valid personnel and sites
                var personnel = await _context.Personnel
                    .Include(p => p.Site)
                    .Where(p => p.Site.CompanyId == selectedCompanyId.Value)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync();

                var domains = await _context.Domains
                    .Where(d => d.CompanyId == selectedCompanyId.Value)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                instructionSheet.Cells[12, 1].Value = "GEÇERLİ PERSONELLER:";
                instructionSheet.Cells[12, 1].Style.Font.Bold = true;
                int row = 13;
                foreach (var person in personnel)
                {
                    string status = person.IsActive ? "Aktif" : "Pasif";
                    instructionSheet.Cells[row, 1].Value = $"{person.FullName} ({person.Site.Name}) - {status}";
                    row++;
                }

                row += 2;
                instructionSheet.Cells[row, 1].Value = "GEÇERLİ DOMAIN'LER:";
                instructionSheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                foreach (var domain in domains)
                {
                    instructionSheet.Cells[row, 1].Value = domain.Name;
                    row++;
                }

                instructionSheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"MailEntries_Template_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Şablon oluşturulurken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

     [Authorize]
public async Task<IActionResult> ExportToExcel(bool showArchived = false, int? siteId = null)
{
    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    if (!selectedCompanyId.HasValue) // selectedCompanyId null ise
    {
        return RedirectToAction("Login", "Account");
    }

    var query = _context.MailEntries
        .Include(m => m.Personnel)
        .ThenInclude(p => p!.Site)
        .Include(m => m.Domain)
        .Where(m => m.Personnel!.Site.CompanyId == selectedCompanyId.Value); // Sorguyu da güncelledik

    if (!showArchived)
    {
        query = query.Where(m => !m.IsArchived);
    }

    if (siteId.HasValue)
    {
        query = query.Where(m => m.Personnel!.SiteId == siteId.Value);
    }

    var mailEntries = await query.ToListAsync();

    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add("Mail Kayıtları");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Sıra No";
                worksheet.Cells[1, 2].Value = "Adı Soyadı";
                worksheet.Cells[1, 3].Value = "Şantiye";
                worksheet.Cells[1, 4].Value = "E-posta Adresi";
                worksheet.Cells[1, 5].Value = "Şifre";
                worksheet.Cells[1, 6].Value = "Domain";
                worksheet.Cells[1, 7].Value = "Arşivlenmiş";

                // Başlık stilini ayarla
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Verileri ekle
                for (int i = 0; i < mailEntries.Count(); i++)
                {
                    var item = mailEntries[i];
                    int row = i + 2;

                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 2].Value = item.Personnel?.FullName;
                    worksheet.Cells[row, 3].Value = item.Personnel?.Site?.Name;
                    worksheet.Cells[row, 4].Value = item.EmailAddress;
                    worksheet.Cells[row, 5].Value = item.Password;
                    worksheet.Cells[row, 6].Value = item.Domain?.Name;
                    worksheet.Cells[row, 7].Value = item.IsArchived ? "Evet" : "Hayır";
                }

                // Sütun genişliklerini ayarla
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"MailKayitlari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}