using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InfoPoint.Services;
using InfoPoint.Models;
using InfoPoint.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InfoPoint.Areas.PDRs.Controllers
{
    [Area("PDRs")]
    [Authorize]
    public class PDRsController : Controller
    {
        private readonly IPDRService _pdrService;
        private readonly ApplicationDbContext _context;
        private readonly IOpenAIService _openAIService;

        public PDRsController(IPDRService pdrService, ApplicationDbContext context, IOpenAIService openAIService)
        {
            _pdrService = pdrService;
            _context = context;
            _openAIService = openAIService;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account", new { area = "" });

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (staff == null)
                return View("NotAuthorized");

            var staffPDRs = await _pdrService.GetStaffPDRsAsync(staff.StaffReference);
            var managerPDRs = await _pdrService.GetManagerPDRsAsync(userEmail);

            var viewModel = new PDRIndexViewModel
            {
                Staff = staff,
                StaffPDRs = staffPDRs,
                ManagerPDRs = managerPDRs
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Complete(int id, PDRType type)
        {
            // Debug: Log what we received
            Console.WriteLine($"DEBUG: Complete action called with id={id}, type={type}");
            ViewBag.DebugInfo = $"Controller received: id={id}, type={type}";
            
            var pdr = await _context.PDRs
                .Include(p => p.Staff)
                .Include(p => p.Responses.Where(r => r.ResponseType == type))
                    .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pdr == null)
                return NotFound();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isStaffMember = pdr.Staff.Email == userEmail;
            var isManager = pdr.Staff.ManagerEmail == userEmail;

            if (type == PDRType.Staff && !isStaffMember)
                return Forbid();
            if (type == PDRType.Manager && !isManager)
                return Forbid();

            var questions = await _pdrService.GetActiveQuestionsAsync();

            var viewModel = new PDRCompletionViewModel
            {
                PDR = pdr,
                Questions = questions,
                ResponseType = type,
                Responses = pdr.Responses.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveResponse(int pdrId, int questionId, PDRType responseType, string response, int? rating, string? notes)
        {
            try
            {
                await _pdrService.SaveResponseAsync(pdrId, questionId, responseType, response, rating, notes);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompletePDR(int pdrId, PDRType responseType)
        {
            var pdr = await _context.PDRs.FindAsync(pdrId);
            if (pdr == null)
                return NotFound();

            var newStatus = responseType == PDRType.Staff ? PDRStatus.StaffCompleted : PDRStatus.ManagerCompleted;
            
            if (responseType == PDRType.Manager && pdr.Status == PDRStatus.StaffCompleted)
            {
                newStatus = PDRStatus.ReadyForCollaboration;
                await _pdrService.CreateComparisonAsync(pdrId);
            }

            await _pdrService.CompletePDRStageAsync(pdrId, newStatus);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Compare(int id)
        {
            var pdr = await _context.PDRs
                .Include(p => p.Staff)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pdr == null)
                return NotFound();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isStaffMember = pdr.Staff.Email == userEmail;
            var isManager = pdr.Staff.ManagerEmail == userEmail;

            if (!isStaffMember && !isManager)
                return Forbid();

            var comparisons = await _context.PDRComparisons
                .Include(c => c.Question)
                .Where(c => c.PDRId == id)
                .OrderBy(c => c.Question.Category)
                .ThenBy(c => c.Question.Order)
                .ToListAsync();

            var viewModel = new PDRComparisonViewModel
            {
                PDR = pdr,
                Comparisons = comparisons,
                IsStaffMember = isStaffMember,
                IsManager = isManager
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCollaborativeResponse(int pdrId, int questionId, string response, int? rating, string? notes)
        {
            try
            {
                // Save to PDRResponses table
                await _pdrService.SaveResponseAsync(pdrId, questionId, PDRType.Collaborative, response, rating, notes);
                
                // Also update the PDRComparison table
                var comparison = await _context.PDRComparisons
                    .FirstOrDefaultAsync(c => c.PDRId == pdrId && c.QuestionId == questionId);
                
                if (comparison != null)
                {
                    comparison.CollaborativeResponse = response;
                    comparison.CollaborativeRating = rating;
                    comparison.ComparisonNotes = notes;
                    await _context.SaveChangesAsync();
                }
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteCollaborative(int pdrId, string smartTargets)
        {
            try
            {
                // Parse and save smart targets
                if (!string.IsNullOrEmpty(smartTargets))
                {
                    var targets = System.Text.Json.JsonSerializer.Deserialize<List<SmartTargetDto>>(smartTargets);
                    if (targets != null && targets.Count >= 2)
                    {
                        foreach (var target in targets)
                        {
                            var smartTarget = new SmartTarget
                            {
                                PDRId = pdrId,
                                Title = target.Title,
                                Description = target.Description,
                                Priority = target.Priority,
                                Category = target.Category,
                                TargetDate = DateTime.Parse(target.TargetDate),
                                SuccessCriteria = target.SuccessCriteria,
                                ActionPlan = target.ActionPlan,
                                Specific = target.Specific,
                                Measurable = target.Measurable,
                                Achievable = target.Achievable,
                                Relevant = target.Relevant,
                                TimeBound = target.TimeBound,
                                DisplayOrder = target.DisplayOrder,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true,
                                Status = "Pending"
                            };

                            _context.SmartTargets.Add(smartTarget);
                        }

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return Json(new { success = false, message = "At least 2 SMART targets are required." });
                    }
                }

                await _pdrService.CompletePDRStageAsync(pdrId, PDRStatus.Completed);
                return Ok();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnhanceSmartTarget([FromBody] EnhanceTargetRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Target))
                {
                    return Json(new { success = false, message = "Target text is required" });
                }

                var enhancedTarget = await _openAIService.EnhanceSmartTarget(request.Target);
                return Json(new { success = true, enhancedTarget });
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                Console.WriteLine($"Error enhancing target: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = "Unable to enhance target at this time. Please try again later." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignPDR(string staffReference, int year, int month = 0, string period = "")
        {
            try
            {
                if (month == 0)
                    month = DateTime.Now.Month;
                
                await _pdrService.AssignPDRAsync(staffReference, year, month, period);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignTeamPDRs(int year, int month, string period = "", string dueDate = "")
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "User not authenticated" });

                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == userEmail);
                if (staff == null)
                    return Json(new { success = false, message = "Staff record not found" });

                // Get all staff members that report to this manager
                var teamMembers = await _context.Staff
                    .Where(s => s.ManagerEmail == userEmail)
                    .ToListAsync();

                if (!teamMembers.Any())
                    return Json(new { success = false, message = "No team members found" });

                DateTime? parsedDueDate = null;
                if (!string.IsNullOrEmpty(dueDate))
                {
                    if (DateTime.TryParse(dueDate, out var due))
                        parsedDueDate = due;
                }

                var assignedCount = 0;
                var existingCount = 0;

                foreach (var teamMember in teamMembers)
                {
                    var existingPDR = await _context.PDRs
                        .FirstOrDefaultAsync(p => p.StaffReference == teamMember.StaffReference && 
                                                p.Year == year && p.Month == month);
                    
                    if (existingPDR == null)
                    {
                        await _pdrService.AssignPDRAsync(teamMember.StaffReference, year, month, period, parsedDueDate);
                        assignedCount++;
                    }
                    else
                    {
                        existingCount++;
                    }
                }

                var message = $"Successfully assigned PDRs to {assignedCount} team members.";
                if (existingCount > 0)
                    message += $" {existingCount} already had PDRs for this period.";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Temporary action for testing - remove in production
        [HttpGet]
        public async Task<IActionResult> ResetTestPDRs()
        {
            if (!User.Identity.IsAuthenticated || User.Identity.Name != "oliver.hill@g.bdc.ac.uk")
            {
                return Forbid();
            }

            await _pdrService.ResetTestPDRsAsync();
            return RedirectToAction(nameof(Index));
        }
    }

    public class PDRIndexViewModel
    {
        public Staff Staff { get; set; } = null!;
        public IEnumerable<PDR> StaffPDRs { get; set; } = new List<PDR>();
        public IEnumerable<PDR> ManagerPDRs { get; set; } = new List<PDR>();
    }

    public class PDRCompletionViewModel
    {
        public PDR PDR { get; set; } = null!;
        public IEnumerable<PDRQuestion> Questions { get; set; } = new List<PDRQuestion>();
        public PDRType ResponseType { get; set; }
        public IList<PDRResponse> Responses { get; set; } = new List<PDRResponse>();
    }

    public class PDRComparisonViewModel
    {
        public PDR PDR { get; set; } = null!;
        public IEnumerable<PDRComparison> Comparisons { get; set; } = new List<PDRComparison>();
        public bool IsStaffMember { get; set; }
        public bool IsManager { get; set; }
    }

    public class SmartTargetDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TargetDate { get; set; } = string.Empty;
        public string? SuccessCriteria { get; set; }
        public string? ActionPlan { get; set; }
        public bool Specific { get; set; }
        public bool Measurable { get; set; }
        public bool Achievable { get; set; }
        public bool Relevant { get; set; }
        public bool TimeBound { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class EnhanceTargetRequest
    {
        public string Target { get; set; } = string.Empty;
    }
}