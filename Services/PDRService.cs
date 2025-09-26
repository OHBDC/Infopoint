using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;
using InfoPoint.Models;

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
            if (month == 0)
                month = DateTime.Now.Month;

            var existingPDR = await _context.PDRs
                .FirstOrDefaultAsync(p => p.StaffReference == staffReference && p.Year == year && p.Month == month);
            
            if (existingPDR != null)
            {
                return existingPDR;
            }

            var pdr = new PDR
            {
                StaffReference = staffReference,
                Year = year,
                Month = month,
                Period = string.IsNullOrEmpty(period) ? null : period,
                Status = PDRStatus.Assigned,
                AssignedDate = DateTime.UtcNow,
                DueDate = dueDate ?? DateTime.UtcNow.AddDays(30)
            };

            _context.PDRs.Add(pdr);
            await _context.SaveChangesAsync();
            return pdr;
        }

        public async Task<PDR?> GetPDRAsync(string staffReference, int year)
        {
            return await _context.PDRs
                .Include(p => p.Staff)
                .Include(p => p.Responses)
                    .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(p => p.StaffReference == staffReference && p.Year == year);
        }

        public async Task<IEnumerable<PDR>> GetStaffPDRsAsync(string staffReference)
        {
            return await _context.PDRs
                .Include(p => p.Staff)
                .Where(p => p.StaffReference == staffReference)
                .OrderByDescending(p => p.Year)
                .ToListAsync();
        }

        public async Task<IEnumerable<PDR>> GetManagerPDRsAsync(string managerEmail)
        {
            return await _context.PDRs
                .Include(p => p.Staff)
                .Where(p => p.Staff.ManagerEmail == managerEmail)
                .OrderByDescending(p => p.Year)
                .ThenBy(p => p.Staff.LastName)
                .ToListAsync();
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
            await Data.Seeders.TestPDRSeeder.ResetAndSeedTestPDRsAsync(_context);
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
    }
}