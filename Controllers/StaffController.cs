using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;

namespace InfoPoint.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staff = await _context.Staff
                .Where(s => s.IsActive)
                .OrderBy(s => s.Area)
                .ThenBy(s => s.LastName)
                .ToListAsync();
            
            return View(staff);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (staff == null)
                return NotFound();

            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> GetByArea(string area)
        {
            var staff = await _context.Staff
                .Where(s => s.IsActive && s.Area == area)
                .OrderBy(s => s.LastName)
                .ToListAsync();
            
            return Json(staff);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<object>());

            var staff = await _context.Staff
                .Where(s => s.IsActive && 
                    (s.FirstName.Contains(term) || 
                     s.LastName.Contains(term) || 
                     s.Email.Contains(term) ||
                     s.StaffReference.Contains(term)))
                .Select(s => new { 
                    s.Id, s.FullName, s.Email, s.StaffReference, s.Area, s.JobTitle 
                })
                .Take(10)
                .ToListAsync();
            
            return Json(staff);
        }
    }
}