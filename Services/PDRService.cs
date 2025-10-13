using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;
using InfoPoint.Models;
using InfoPoint.Extensions;

namespace InfoPoint.Services
{
    public interface IPDRService
    {
        Task<PDR> AssignPDRAsync(string staffReference, int year, int month = 0, string period = "", DateTime? dueDate = null);
        Task<PDR?> GetPDRAsync(string staffReference, int year);
        Task<IEnumerable<PDR>> GetStaffPDRsAsync(string staffReference);
        Task<IEnumerable<PDR>> GetManagerPDRsAsync(string managerEmail);
        Task<PDRResponse> SaveResponseAsync(int pdrId, int questionId, PDRType responseType, string response, int? rating = null, string? notes = null);
        Task<PDR> CompletePDRStageAsync(int pdrId, PDRStatus newStatus);
        Task<PDRComparison> CreateComparisonAsync(int pdrId);
        Task<IEnumerable<PDRQuestion>> GetActiveQuestionsAsync();
        Task SeedQuestionsAsync();
        Task ResetTestPDRsAsync();
    }

    public class PDRService : IPDRService
    {
        private readonly ApplicationDbContext _context;

        public PDRService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PDR> AssignPDRAsync(string staffReference, int year, int month = 0, string period = "", DateTime? dueDate = null)
        {
            // Smart period detection: if no period provided, get current period
            if (string.IsNullOrEmpty(period))
            {
                period = PDRPeriod.GetCurrentPeriod();
            }

            // Validate period
            if (!PDRPeriod.IsValidPeriod(period))
            {
                throw new ArgumentException($"Invalid PDR period: {period}. Must be one of: {string.Join(", ", PDRPeriod.AllPeriods)}");
            }

            // Set month based on period start month if not provided
            if (month == 0)
            {
                month = PDRPeriod.GetStartMonth(period);
            }

            // Check for existing PDR with same staff, year, and period
            var existingPDR = await _context.PDRs
                .FirstOrDefaultAsync(p => p.StaffReference == staffReference &&
                                        p.Year == year &&
                                        p.Period == period);

            if (existingPDR != null)
            {
                return existingPDR;
            }

            // Set due date based on period if not provided
            if (!dueDate.HasValue)
            {
                dueDate = PDRPeriod.GetDueDate(period, year);
            }

            var pdr = new PDR
            {
                StaffReference = staffReference,
                Year = year,
                Month = month,
                Period = period,
                Status = PDRStatus.Assigned,
                AssignedDate = DateTime.UtcNow,
                DueDate = dueDate.Value
            };

            _context.PDRs.Add(pdr);
            await _context.SaveChangesAsync();
            return pdr;
        }

        public async Task<PDR?> GetPDRAsync(string staffReference, int year)
        {
            var pdr = await _context.PDRs
                .Include(p => p.Responses)
                    .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(p => p.StaffReference == staffReference && p.Year == year);

            if (pdr != null)
            {
                await LoadStaffAsync(pdr);
            }

            return pdr;
        }

        public async Task<IEnumerable<PDR>> GetStaffPDRsAsync(string staffReference)
        {
            var pdrs = await _context.PDRs
                .Where(p => p.StaffReference == staffReference)
                .OrderByDescending(p => p.Year)
                .ToListAsync();

            await LoadStaffForPDRsAsync(pdrs);

            return pdrs;
        }

        public async Task<IEnumerable<PDR>> GetManagerPDRsAsync(string managerEmail)
        {
            // Get current user's employee number from their email
            // Use FindByEmailAsync to handle @bdc.ac.uk / @g.bdc.ac.uk equivalence
            var manager = await _context.Staff.FindByEmailAsync(managerEmail);

            Console.WriteLine($"DEBUG GetManagerPDRsAsync: Looking for manager with email={managerEmail}");
            if (manager == null)
            {
                Console.WriteLine($"DEBUG GetManagerPDRsAsync: Manager not found!");
                return new List<PDR>();
            }
            Console.WriteLine($"DEBUG GetManagerPDRsAsync: Found manager ID={manager.Id}, Name={manager.FullName}");

            // Get all staff who report to this manager from HR hierarchy system
            // This uses the proper organizational structure from StaffHR table
            var directReports = await _context.StaffHR
                .Where(hr => hr.ManagerEmployeeNumber == manager.Id && hr.IsActive)
                .ToListAsync();

            Console.WriteLine($"DEBUG GetManagerPDRsAsync: Found {directReports.Count} direct reports from HR system");

            // If no direct reports in HR system, fall back to old AD Manager field
            // This ensures backwards compatibility
            if (!directReports.Any())
            {
                var staffMembersAD = await _context.Staff
                    .Where(s => s.Manager == managerEmail)
                    .ToListAsync();

                var staffReferencesAD = staffMembersAD
                    .Select(s => s.Id.ToString().PadLeft(8, '0'))
                    .ToList();

                var pdrsAD = await _context.PDRs
                    .Where(p => staffReferencesAD.Contains(p.StaffReference))
                    .OrderByDescending(p => p.Year)
                    .ToListAsync();

                await LoadStaffForPDRsAsync(pdrsAD);

                return pdrsAD
                    .OrderByDescending(p => p.Year)
                    .ThenBy(p => p.Staff?.LastName ?? "")
                    .ToList();
            }

            // Get staff references from HR system
            var staffReferences = directReports
                .Select(s => s.EmployeeNumber.ToString().PadLeft(8, '0'))
                .ToList();

            Console.WriteLine($"DEBUG GetManagerPDRsAsync: Staff references: {string.Join(", ", staffReferences)}");

            // Get PDRs for those staff members
            var pdrs = await _context.PDRs
                .Where(p => staffReferences.Contains(p.StaffReference))
                .OrderByDescending(p => p.Year)
                .ToListAsync();

            Console.WriteLine($"DEBUG GetManagerPDRsAsync: Found {pdrs.Count} PDRs for these staff members");

            await LoadStaffForPDRsAsync(pdrs);

            // Sort by year then last name
            return pdrs
                .OrderByDescending(p => p.Year)
                .ThenBy(p => p.Staff?.LastName ?? "")
                .ToList();
        }

        public async Task<PDRResponse> SaveResponseAsync(int pdrId, int questionId, PDRType responseType, string response, int? rating = null, string? notes = null)
        {
            var existingResponse = await _context.PDRResponses
                .FirstOrDefaultAsync(r => r.PDRId == pdrId && r.QuestionId == questionId && r.ResponseType == responseType);

            if (existingResponse != null)
            {
                existingResponse.Response = response;
                existingResponse.Rating = rating;
                existingResponse.Notes = notes;
                existingResponse.ResponseDate = DateTime.UtcNow;
            }
            else
            {
                existingResponse = new PDRResponse
                {
                    PDRId = pdrId,
                    QuestionId = questionId,
                    ResponseType = responseType,
                    Response = response,
                    Rating = rating,
                    Notes = notes
                };
                _context.PDRResponses.Add(existingResponse);
            }

            await _context.SaveChangesAsync();
            return existingResponse;
        }

        public async Task<PDR> CompletePDRStageAsync(int pdrId, PDRStatus newStatus)
        {
            var pdr = await _context.PDRs.FindAsync(pdrId);
            if (pdr == null)
                throw new ArgumentException("PDR not found", nameof(pdrId));

            pdr.Status = newStatus;
            pdr.LastUpdated = DateTime.UtcNow;

            switch (newStatus)
            {
                case PDRStatus.StaffCompleted:
                    pdr.StaffCompletedDate = DateTime.UtcNow;
                    break;
                case PDRStatus.ManagerCompleted:
                    pdr.ManagerCompletedDate = DateTime.UtcNow;
                    break;
                case PDRStatus.Completed:
                    pdr.CollaborativeCompletedDate = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();
            return pdr;
        }

        public async Task<PDRComparison> CreateComparisonAsync(int pdrId)
        {
            var questions = await GetActiveQuestionsAsync();
            
            foreach (var question in questions)
            {
                var staffResponse = await _context.PDRResponses
                    .FirstOrDefaultAsync(r => r.PDRId == pdrId && r.QuestionId == question.Id && r.ResponseType == PDRType.Staff);
                
                var managerResponse = await _context.PDRResponses
                    .FirstOrDefaultAsync(r => r.PDRId == pdrId && r.QuestionId == question.Id && r.ResponseType == PDRType.Manager);

                var existingComparison = await _context.PDRComparisons
                    .FirstOrDefaultAsync(c => c.PDRId == pdrId && c.QuestionId == question.Id);

                if (existingComparison == null)
                {
                    var comparison = new PDRComparison
                    {
                        PDRId = pdrId,
                        QuestionId = question.Id,
                        StaffResponse = staffResponse?.Response,
                        ManagerResponse = managerResponse?.Response,
                        StaffRating = staffResponse?.Rating,
                        ManagerRating = managerResponse?.Rating
                    };
                    _context.PDRComparisons.Add(comparison);
                }
            }

            await _context.SaveChangesAsync();
            
            return await _context.PDRComparisons
                .Include(c => c.Question)
                .FirstAsync(c => c.PDRId == pdrId);
        }

        public async Task<IEnumerable<PDRQuestion>> GetActiveQuestionsAsync()
        {
            return await _context.PDRQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.Order)
                .ToListAsync();
        }

        public async Task ResetTestPDRsAsync()
        {
            // Delete all PDR-related data to allow fresh assignments
            var smartTargets = await _context.SmartTargets.ToListAsync();
            _context.SmartTargets.RemoveRange(smartTargets);

            var comparisons = await _context.PDRComparisons.ToListAsync();
            _context.PDRComparisons.RemoveRange(comparisons);

            var responses = await _context.PDRResponses.ToListAsync();
            _context.PDRResponses.RemoveRange(responses);

            var pdrs = await _context.PDRs.ToListAsync();
            _context.PDRs.RemoveRange(pdrs);

            await _context.SaveChangesAsync();
        }

        public async Task SeedQuestionsAsync()
        {
            if (await _context.PDRQuestions.AnyAsync())
                return;

            var questions = new List<PDRQuestion>
            {
                new PDRQuestion
                {
                    Id = 1,
                    StaffQuestionText = "How effectively are you balancing workload and wellbeing (e.g. stress levels, energy, work-life balance)?",
                    ManagerQuestionText = "How effectively is the staff member balancing workload and wellbeing (e.g. stress levels, energy, work-life balance)?",
                    Category = "Wellbeing",
                    Order = 1,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 2,
                    StaffQuestionText = "How supported do you feel in your role (e.g. by colleagues, manager, or wider organisation)?",
                    ManagerQuestionText = "How supported does the staff member appear to feel in their role (e.g. by colleagues, manager, or wider organisation)?",
                    Category = "Wellbeing",
                    Order = 2,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 3,
                    StaffQuestionText = "What one change would most improve your day-to-day wellbeing at work?",
                    ManagerQuestionText = null,
                    Category = "Wellbeing",
                    Order = 3,
                    HasManagerQuestion = false
                },
                new PDRQuestion
                {
                    Id = 4,
                    StaffQuestionText = "How have you progressed in developing your skills and knowledge over the past year?",
                    ManagerQuestionText = "How has the staff member progressed in developing their skills and knowledge over the past year?",
                    Category = "Development",
                    Order = 4,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 5,
                    StaffQuestionText = "What key skills or experiences do you want to focus on developing in the coming year?",
                    ManagerQuestionText = "What key skills or experiences should the staff member focus on developing in the coming year?",
                    Category = "Development",
                    Order = 5,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 6,
                    StaffQuestionText = "What development opportunities (training, projects, mentoring) would you most value in the next 12 months?",
                    ManagerQuestionText = null,
                    Category = "Development",
                    Order = 6,
                    HasManagerQuestion = false
                },
                new PDRQuestion
                {
                    Id = 7,
                    StaffQuestionText = "How effectively have you met your objectives and contributed to team/organisational goals?",
                    ManagerQuestionText = "How effectively has the staff member met their objectives and contributed to team/organisational goals?",
                    Category = "Performance",
                    Order = 7,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 8,
                    StaffQuestionText = "What are your main strengths and how do they positively impact your role?",
                    ManagerQuestionText = "What are the staff member's main strengths and how do they positively impact their role?",
                    Category = "Performance",
                    Order = 8,
                    HasManagerQuestion = true
                },
                new PDRQuestion
                {
                    Id = 9,
                    StaffQuestionText = "Which areas of your performance do you feel need the most improvement, and what support would help?",
                    ManagerQuestionText = null,
                    Category = "Performance",
                    Order = 9,
                    HasManagerQuestion = false
                },
                new PDRQuestion
                {
                    Id = 10,
                    StaffQuestionText = "What achievements are you most proud of this year, and why?",
                    ManagerQuestionText = null,
                    Category = "Performance",
                    Order = 10,
                    HasManagerQuestion = false
                }
            };

            _context.PDRQuestions.AddRange(questions);
            await _context.SaveChangesAsync();
        }

        // Helper methods to manually load Staff navigation property
        private async Task LoadStaffAsync(PDR pdr)
        {
            if (!string.IsNullOrEmpty(pdr.StaffReference))
            {
                // StaffReference is 8-digit padded string, need to convert to int
                if (int.TryParse(pdr.StaffReference, out int staffId))
                {
                    pdr.Staff = (await _context.Staff.FindAsync(staffId))!;
                }
            }
        }

        private async Task LoadStaffForPDRsAsync(IEnumerable<PDR> pdrs)
        {
            var staffReferences = pdrs
                .Where(p => !string.IsNullOrEmpty(p.StaffReference))
                .Select(p => p.StaffReference)
                .Distinct()
                .ToList();

            var staffIds = staffReferences
                .Select(sr => int.TryParse(sr, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            var staffMembers = await _context.Staff
                .Where(s => staffIds.Contains(s.Id))
                .ToListAsync();

            var staffLookup = staffMembers.ToDictionary(
                s => s.Id.ToString().PadLeft(8, '0'),
                s => s
            );

            foreach (var pdr in pdrs)
            {
                if (!string.IsNullOrEmpty(pdr.StaffReference) &&
                    staffLookup.TryGetValue(pdr.StaffReference, out var staff))
                {
                    pdr.Staff = staff;
                }
            }
        }
    }
}