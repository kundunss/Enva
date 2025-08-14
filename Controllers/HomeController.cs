using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemApp.Data;
using SistemApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SistemApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Dashboard summary data
        ViewData["SiteCount"] = await _context.Sites.CountAsync();
        ViewData["PersonnelCount"] = await _context.Personnel.CountAsync();
        ViewData["ActivePersonnelCount"] = await _context.Personnel.Where(p => p.IsActive).CountAsync();
        ViewData["SystemHardwareCount"] = await _context.SystemHardware.CountAsync();
        ViewData["MailEntryCount"] = await _context.MailEntries.Where(m => !m.IsArchived).CountAsync();
        ViewData["InventoryItemCount"] = await _context.InventoryItems.Where(i => !i.IsArchived).CountAsync();
        
        // Recent activity (last 5 inventory assignments)
        var recentAssignments = await _context.InventoryAssignmentHistory
            .Include(h => h.InventoryItem)
            .ThenInclude(i => i.EquipmentType)
            .Include(h => h.Personnel)
            .OrderByDescending(h => h.AssignedDate)
            .Take(5)
            .ToListAsync();
        
        return View(recentAssignments);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
