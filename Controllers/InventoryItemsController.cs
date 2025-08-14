using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace SistemApp.Controllers
{
    [Authorize]
    public class InventoryItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryItemsController(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.License.SetNonCommercialPersonal("Enva");
        }

        // GET: InventoryItems
        public async Task<IActionResult> Index(bool showArchived = false, int? siteId = null)
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<InventoryItem> query = _context.InventoryItems
                .Include(i => i.EquipmentType)
                .Include(i => i.Personnel)
                .Include(i => i.Personnel.Site)
                .Where(i => i.Personnel.Site.CompanyId == selectedCompanyId.Value);

            if (!showArchived)
            {
                query = query.Where(i => !i.IsArchived);
            }

            if (siteId.HasValue)
            {
                query = query.Where(i => i.Personnel.SiteId == siteId.Value);
            }

            var inventoryItems = await query
                .OrderBy(i => i.Personnel.Site.Name ?? "")
                .ThenBy(i => i.Personnel.FirstName ?? "")
                .ToListAsync();

            var equipmentTypeCounts = inventoryItems
                .GroupBy(i => i.EquipmentType.Name)
                .Select(g => new { EquipmentType = g.Key, Count = g.Count() })
                .OrderBy(x => x.EquipmentType)
                .ToList();

            ViewData["ShowArchived"] = showArchived;
            ViewBag.Sites = await _context.Sites
                .Where(s => s.CompanyId == selectedCompanyId.Value)
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.SelectedSiteId = siteId;
            ViewBag.EquipmentTypeCounts = equipmentTypeCounts;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

            return View(inventoryItems);
        }

        // GET: InventoryItems/Details/5
        public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    var inventoryItem = await _context.InventoryItems
        .Include(i => i.EquipmentType)
        .Include(i => i.Personnel)
            .ThenInclude(p => p.Site)
        .Include(i => i.AssignmentHistory)
            .ThenInclude(h => h.Personnel)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (inventoryItem == null) return NotFound();

    ViewBag.QrCodeUrl = Url.Action("Details", "InventoryItems", new { id = inventoryItem.Id }, Request.Scheme);

    return View(inventoryItem);
}

        // CREATE - GET
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name");
            ViewData["PersonnelId"] = new SelectList(
                _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
                    .Include(p => p.Site)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
                "Id", "FullInfo");

            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,EquipmentTypeId,SerialNumber,Description,PersonnelId,IsArchived")] InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventoryItem);
                await _context.SaveChangesAsync();

                if (inventoryItem.PersonnelId > 0)
                {
                    _context.InventoryAssignmentHistory.Add(new InventoryAssignmentHistory
                    {
                        InventoryItemId = inventoryItem.Id,
                        PersonnelId = inventoryItem.PersonnelId,
                        AssignedDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }

            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (selectedCompanyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["EquipmentTypeId"] = new SelectList(_context.EquipmentTypes.OrderBy(e => e.Name), "Id", "Name", inventoryItem.EquipmentTypeId);
            ViewData["PersonnelId"] = new SelectList(
                _context.Personnel.Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
                    .Include(p => p.Site)
                    .OrderBy(p => p.Site.Name)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site.Name})" }),
                "Id", "FullInfo", inventoryItem.PersonnelId);

            ViewBag.SelectedCompanyId = selectedCompanyId.Value;
            ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

            return View(inventoryItem);
        }
// EDIT - GET
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int? id)
{
    if (id == null) return NotFound();

    var inventoryItem = await _context.InventoryItems
        .Include(i => i.EquipmentType)
        .Include(i => i.Personnel)
            .ThenInclude(p => p.Site)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (inventoryItem == null) return NotFound();

    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    if (selectedCompanyId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // EquipmentType dropdown
    ViewData["EquipmentTypeId"] = new SelectList(
        _context.EquipmentTypes.OrderBy(e => e.Name),
        "Id", "Name", inventoryItem.EquipmentTypeId
    );

    // Personnel dropdown (FullName ve Site client-side sıralama ile)
    ViewData["PersonnelId"] = new SelectList(
        _context.Personnel
            .Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
            .Include(p => p.Site)
            .AsEnumerable() // client-side
            .OrderBy(p => p.Site?.Name)
            .ThenBy(p => p.FullName)
            .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site?.Name})" }),
        "Id", "FullInfo", inventoryItem.PersonnelId
    );

    ViewBag.SelectedCompanyId = selectedCompanyId.Value;
    ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

    return View(inventoryItem);
}

// EDIT - POST
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int id, [Bind("Id,EquipmentTypeId,SerialNumber,Description,PersonnelId,IsArchived")] InventoryItem inventoryItem)
{
    if (id != inventoryItem.Id) return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(inventoryItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventoryItemExists(inventoryItem.Id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
    if (selectedCompanyId == null) return RedirectToAction("Login", "Account");

    ViewData["EquipmentTypeId"] = new SelectList(
        _context.EquipmentTypes.OrderBy(e => e.Name),
        "Id", "Name", inventoryItem.EquipmentTypeId
    );

    ViewData["PersonnelId"] = new SelectList(
        _context.Personnel
            .Where(p => p.IsActive && p.Site.CompanyId == selectedCompanyId.Value)
            .Include(p => p.Site)
            .AsEnumerable()
            .OrderBy(p => p.Site?.Name)
            .ThenBy(p => p.FullName)
            .Select(p => new { p.Id, FullInfo = $"{p.FullName} ({p.Site?.Name})" }),
        "Id", "FullInfo", inventoryItem.PersonnelId
    );

    ViewBag.SelectedCompanyId = selectedCompanyId.Value;
    ViewBag.SelectedCompanyName = HttpContext.Session.GetString("SelectedCompanyName");

    return View(inventoryItem);
}
// GET: InventoryItems/Delete/5
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(int? id)
{
    if (id == null) return NotFound();

    var inventoryItem = await _context.InventoryItems
        .Include(i => i.EquipmentType)
        .Include(i => i.Personnel)
            .ThenInclude(p => p.Site)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (inventoryItem == null) return NotFound();

    return View(inventoryItem);
}

// POST: InventoryItems/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var inventoryItem = await _context.InventoryItems.FindAsync(id);
    if (inventoryItem == null) return NotFound();

    _context.InventoryItems.Remove(inventoryItem);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}
        // QR kod resmi üreten yeni metod (çakışma yok)
        [HttpGet]
public IActionResult GenerateQRCode(string url)
{
    if (string.IsNullOrEmpty(url))
    {
        return BadRequest("QR kod için URL bulunamadı!");
    }

    using (var qrGenerator = new QRCoder.QRCodeGenerator())
    {
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
        var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        return File(qrCodeBytes, "image/png");
    }
}

        // QR Kod yazdırma sayfası
        public IActionResult PrintQRCode(int id)
{
    var item = _context.InventoryItems
        .Include(i => i.EquipmentType)
        .Include(i => i.Personnel)
            .ThenInclude(p => p.Site)
        .FirstOrDefault(i => i.Id == id);

    if (item == null) return NotFound();

    ViewBag.QrCodeUrl = Url.Action("Details", "InventoryItems", new { id = item.Id }, Request.Scheme);


    return View(item);
}	


        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.Id == id);
        }
    }
}
