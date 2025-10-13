using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InfoPoint.Services;
using InfoPoint.Models;
using InfoPoint.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InfoPoint.Extensions;

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

            var staff = await _context.Staff.FindByEmailAsync(userEmail);
            if (staff == null)
                return View("NotAuthorized");

            var staffPDRs = await _pdrService.GetStaffPDRsAsync(staff.StaffReference);
            var managerPDRs = await _pdrService.GetManagerPDRsAsync(userEmail);

            // Debug logging
            Console.WriteLine($"DEBUG Index: Staff ID={staff.Id}, Email={userEmail}");
            Console.WriteLine($"DEBUG Index: Found {managerPDRs.Count()} manager PDRs");
            foreach (var pdr in managerPDRs)
            {
                Console.WriteLine($"DEBUG Index: PDR Id={pdr.Id}, StaffRef={pdr.StaffReference}, Year={pdr.Year}, Status={pdr.Status}, Staff={pdr.Staff?.FullName ?? "NULL"}");
            }

            // Group PDRs by year
            var currentYear = DateTime.Now.Year;
            var staffPDRsByYear = staffPDRs.GroupBy(p => p.Year).OrderByDescending(g => g.Key).ToList();
            var managerPDRsByYear = managerPDRs.GroupBy(p => p.Year).OrderByDescending(g => g.Key).ToList();

            // Get all unique years
            var allYears = staffPDRs.Select(p => p.Year)
                .Union(managerPDRs.Select(p => p.Year))
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            // Calculate summary statistics for manager
            var managerStats = new PDRStatistics
            {
                TotalPDRs = managerPDRs.Count(),
                Assigned = managerPDRs.Count(p => p.Status == PDRStatus.Assigned),
                StaffCompleted = managerPDRs.Count(p => p.Status == PDRStatus.StaffCompleted),
                ReadyForCollaboration = managerPDRs.Count(p => p.Status == PDRStatus.ReadyForCollaboration),
                Completed = managerPDRs.Count(p => p.Status == PDRStatus.Completed)
            };

            // Calculate summary statistics for staff
            var staffStats = new PDRStatistics
            {
                TotalPDRs = staffPDRs.Count(),
                Assigned = staffPDRs.Count(p => p.Status == PDRStatus.Assigned),
                StaffCompleted = staffPDRs.Count(p => p.Status == PDRStatus.StaffCompleted),
                ReadyForCollaboration = staffPDRs.Count(p => p.Status == PDRStatus.ReadyForCollaboration),
                Completed = staffPDRs.Count(p => p.Status == PDRStatus.Completed)
            };

            // Check if user has team members (from HR hierarchy)
            var hasTeamMembers = await _context.StaffHR
                .AnyAsync(hr => hr.ManagerEmployeeNumber == staff.Id && hr.IsActive);

            var viewModel = new PDRIndexViewModel
            {
                Staff = staff,
                StaffPDRs = staffPDRs,
                ManagerPDRs = managerPDRs,
                StaffPDRsByYear = staffPDRsByYear,
                ManagerPDRsByYear = managerPDRsByYear,
                AvailableYears = allYears,
                CurrentYear = currentYear,
                ManagerStats = managerStats,
                StaffStats = staffStats,
                HasTeamMembers = hasTeamMembers
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
                .Include(p => p.Responses.Where(r => r.ResponseType == type))
                    .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pdr == null)
                return NotFound();

            // Manually load Staff since the relationship is not managed by EF
            if (int.TryParse(pdr.StaffReference, out int staffId))
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff != null)
                {
                    pdr.Staff = staff;
                }
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isStaffMember = PDREmailHelper.EmailsMatch(pdr.Staff?.Email, userEmail);
            var isManager = PDREmailHelper.EmailsMatch(pdr.Staff?.ManagerEmail, userEmail);

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
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pdr == null)
                return NotFound();

            // Manually load Staff since the relationship is not managed by EF
            if (int.TryParse(pdr.StaffReference, out int staffId))
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff != null)
                {
                    pdr.Staff = staff;
                }
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isStaffMember = PDREmailHelper.EmailsMatch(pdr.Staff?.Email, userEmail);
            var isManager = PDREmailHelper.EmailsMatch(pdr.Staff?.ManagerEmail, userEmail);

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
        public async Task<IActionResult> AssignTeamPDRs(int year = 0, string period = "")
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "User not authenticated" });

                var staff = await _context.Staff.FindByEmailAsync(userEmail);
                if (staff == null)
                    return Json(new { success = false, message = "Staff record not found" });

                // Smart defaults: if no year or period provided, use current values
                if (year == 0)
                    year = DateTime.Now.Year;

                if (string.IsNullOrEmpty(period))
                    period = PDRPeriod.GetCurrentPeriod();

                // Validate period
                if (!PDRPeriod.IsValidPeriod(period))
                    return Json(new { success = false, message = $"Invalid period. Must be one of: {string.Join(", ", PDRPeriod.AllPeriods)}" });

                // Get all staff members that report to this manager from HR hierarchy system
                var directReports = await _context.StaffHR
                    .Where(hr => hr.ManagerEmployeeNumber == staff.Id && hr.IsActive)
                    .ToListAsync();

                // If no direct reports in HR system, fall back to old AD Manager field
                if (!directReports.Any())
                {
                    var teamMembersAD = await _context.Staff
                        .Where(s => s.ManagerEmail == userEmail)
                        .ToListAsync();

                    if (!teamMembersAD.Any())
                        return Json(new { success = false, message = "No team members found - please ensure your team is set up in the HR system" });

                    directReports = teamMembersAD.Select(ad => new StaffHR
                    {
                        EmployeeNumber = ad.Id,
                        FullName = ad.FullName
                    }).ToList();
                }

                var assignedCount = 0;
                var existingCount = 0;

                foreach (var teamMember in directReports)
                {
                    var staffRef = teamMember.EmployeeNumber.ToString().PadLeft(8, '0');
                    var existingPDR = await _context.PDRs
                        .FirstOrDefaultAsync(p => p.StaffReference == staffRef &&
                                                p.Year == year &&
                                                p.Period == period);

                    if (existingPDR == null)
                    {
                        await _pdrService.AssignPDRAsync(staffRef, year, 0, period, null);
                        assignedCount++;
                    }
                    else
                    {
                        existingCount++;
                    }
                }

                var message = $"Successfully assigned {PDRPeriod.GetDisplayName(period)} {year} PDRs to {assignedCount} team members.";
                if (existingCount > 0)
                    message += $" {existingCount} already had PDRs for this period.";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Team Dashboard - shows manager's team members and their PDR status
        [HttpGet]
        public async Task<IActionResult> Team(int? year)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account", new { area = "" });

            var staff = await _context.Staff.FindByEmailAsync(userEmail);
            if (staff == null)
                return View("NotAuthorized");

            // Get all direct reports from HR system
            var directReports = await _context.StaffHR
                .Where(hr => hr.ManagerEmployeeNumber == staff.Id && hr.IsActive)
                .OrderBy(hr => hr.FullName)
                .ToListAsync();

            if (!directReports.Any())
            {
                // No team members found
                ViewBag.Message = "You currently have no direct reports in the HR system.";
                return View(new TeamDashboardViewModel { Manager = staff });
            }

            // Get staff references for all team members
            var staffReferences = directReports
                .Select(s => s.EmployeeNumber.ToString().PadLeft(8, '0'))
                .ToList();

            // Determine which year to show
            int selectedYear = year ?? DateTime.Now.Year;

            // Get all PDRs for team members in selected year
            var teamPDRs = await _context.PDRs
                .Where(p => staffReferences.Contains(p.StaffReference) && p.Year == selectedYear)
                .ToListAsync();

            // Load staff data for PDRs - load all staff in bulk to avoid N+1 queries
            var employeeNumbers = staffReferences.Select(sr => int.Parse(sr)).ToList();
            var staffMembers = await _context.Staff
                .Where(s => employeeNumbers.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id.ToString().PadLeft(8, '0'), s => s);

            // Assign staff to each PDR
            foreach (var pdr in teamPDRs)
            {
                if (staffMembers.ContainsKey(pdr.StaffReference))
                {
                    pdr.Staff = staffMembers[pdr.StaffReference];
                }
            }

            // Get available years (years where team has PDRs)
            var availableYears = await _context.PDRs
                .Where(p => staffReferences.Contains(p.StaffReference))
                .Select(p => p.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Any())
                availableYears.Add(DateTime.Now.Year);

            // Create team member summaries
            var teamMembers = new List<TeamMemberSummary>();
            foreach (var report in directReports)
            {
                var staffRef = report.EmployeeNumber.ToString().PadLeft(8, '0');
                var memberPDRs = teamPDRs.Where(p => p.StaffReference == staffRef).ToList();
                var staffAD = await _context.Staff.FirstOrDefaultAsync(s => s.Id == report.EmployeeNumber);

                teamMembers.Add(new TeamMemberSummary
                {
                    StaffHR = report,
                    StaffAD = staffAD,
                    PDRs = memberPDRs,
                    TotalPDRs = memberPDRs.Count,
                    AssignedCount = memberPDRs.Count(p => p.Status == PDRStatus.Assigned),
                    StaffCompletedCount = memberPDRs.Count(p => p.Status == PDRStatus.StaffCompleted),
                    ManagerCompletedCount = memberPDRs.Count(p => p.Status == PDRStatus.ManagerCompleted),
                    ReadyForCollaborationCount = memberPDRs.Count(p => p.Status == PDRStatus.ReadyForCollaboration),
                    CompletedCount = memberPDRs.Count(p => p.Status == PDRStatus.Completed),
                    OverduePDRs = memberPDRs.Where(p => p.DueDate.HasValue && p.DueDate.Value < DateTime.Now && p.Status != PDRStatus.Completed).ToList(),
                    NeedsManagerAction = memberPDRs.Where(p => p.Status == PDRStatus.StaffCompleted || p.Status == PDRStatus.ReadyForCollaboration).ToList()
                });
            }

            var viewModel = new TeamDashboardViewModel
            {
                Manager = staff,
                TeamMembers = teamMembers,
                SelectedYear = selectedYear,
                AvailableYears = availableYears,
                TotalTeamSize = teamMembers.Count,
                TotalPDRs = teamPDRs.Count,
                TotalOverdue = teamMembers.Sum(t => t.OverduePDRs.Count),
                TotalNeedsAction = teamMembers.Sum(t => t.NeedsManagerAction.Count)
            };

            return View(viewModel);
        }

        // Temporary action for testing - remove in production
        [HttpGet]
        public async Task<IActionResult> ResetTestPDRs()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || User.Identity.Name != "oliver.hill@g.bdc.ac.uk")
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
        public List<IGrouping<int, PDR>> StaffPDRsByYear { get; set; } = new List<IGrouping<int, PDR>>();
        public List<IGrouping<int, PDR>> ManagerPDRsByYear { get; set; } = new List<IGrouping<int, PDR>>();
        public List<int> AvailableYears { get; set; } = new List<int>();
        public int CurrentYear { get; set; }
        public PDRStatistics ManagerStats { get; set; } = new PDRStatistics();
        public PDRStatistics StaffStats { get; set; } = new PDRStatistics();
        public bool HasTeamMembers { get; set; } = false;
    }

    public class PDRStatistics
    {
        public int TotalPDRs { get; set; }
        public int Assigned { get; set; }
        public int StaffCompleted { get; set; }
        public int ReadyForCollaboration { get; set; }
        public int Completed { get; set; }
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

    // Team Dashboard View Models
    public class TeamDashboardViewModel
    {
        public Staff Manager { get; set; } = null!;
        public List<TeamMemberSummary> TeamMembers { get; set; } = new List<TeamMemberSummary>();
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
        public int TotalTeamSize { get; set; }
        public int TotalPDRs { get; set; }
        public int TotalOverdue { get; set; }
        public int TotalNeedsAction { get; set; }
    }

    public class TeamMemberSummary
    {
        public StaffHR StaffHR { get; set; } = null!;
        public Staff? StaffAD { get; set; }
        public List<PDR> PDRs { get; set; } = new List<PDR>();
        public int TotalPDRs { get; set; }
        public int AssignedCount { get; set; }
        public int StaffCompletedCount { get; set; }
        public int ManagerCompletedCount { get; set; }
        public int ReadyForCollaborationCount { get; set; }
        public int CompletedCount { get; set; }
        public List<PDR> OverduePDRs { get; set; } = new List<PDR>();
        public List<PDR> NeedsManagerAction { get; set; } = new List<PDR>();
    }

    // Helper extension for email matching
    public static class PDREmailHelper
    {
        /// <summary>
        /// Checks if two emails match, accounting for @bdc.ac.uk and @g.bdc.ac.uk equivalence
        /// </summary>
        public static bool EmailsMatch(string? email1, string? email2)
        {
            if (string.IsNullOrEmpty(email1) || string.IsNullOrEmpty(email2))
                return false;

            // Direct match
            if (email1.Equals(email2, StringComparison.OrdinalIgnoreCase))
                return true;

            // Convert both to BDC format and compare
            var bdc1 = email1.ToBdcEmail();
            var bdc2 = email2.ToBdcEmail();

            return bdc1?.Equals(bdc2, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}