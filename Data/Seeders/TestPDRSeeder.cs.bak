using InfoPoint.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoPoint.Data.Seeders
{
    public static class TestPDRSeeder
    {
        public static async Task ResetAndSeedTestPDRsAsync(ApplicationDbContext context)
        {
            // Get Oliver Hill's staff record
            var oliverStaff = await context.Staff
                .FirstOrDefaultAsync(s => s.Email == "oliver.hill@g.bdc.ac.uk");
            
            if (oliverStaff == null)
            {
                Console.WriteLine("Oliver Hill staff record not found!");
                return;
            }

            // Delete all existing PDRs
            var existingPDRs = await context.PDRs
                .Include(p => p.Responses)
                .ToListAsync();
            
            var existingComparisons = await context.PDRComparisons.ToListAsync();
            
            context.PDRComparisons.RemoveRange(existingComparisons);
            context.PDRs.RemoveRange(existingPDRs);
            await context.SaveChangesAsync();

            // Get all questions
            var questions = await context.PDRQuestions.OrderBy(q => q.Order).ToListAsync();

            // Create PDRs with different statuses for Oliver as staff member
            await CreatePDRWithStatus(context, oliverStaff.StaffReference, 2024, PDRStatus.Assigned, questions);
            await CreatePDRWithStatus(context, oliverStaff.StaffReference, 2023, PDRStatus.StaffCompleted, questions);
            await CreatePDRWithStatus(context, oliverStaff.StaffReference, 2022, PDRStatus.ReadyForCollaboration, questions);
            await CreatePDRWithStatus(context, oliverStaff.StaffReference, 2021, PDRStatus.Completed, questions);

            // Temporarily make Oliver a manager by updating some staff records
            var staffToManage = await context.Staff
                .Where(s => s.Email.StartsWith("rachel.thompson") || 
                           s.Email.StartsWith("david.brown") || 
                           s.Email.StartsWith("lisa.garcia"))
                .ToListAsync();
            
            foreach (var staff in staffToManage)
            {
                staff.ManagerEmail = oliverStaff.Email;
            }
            await context.SaveChangesAsync();

            // Get staff managed by Oliver
            var managedStaff = await context.Staff
                .Where(s => s.ManagerEmail == oliverStaff.Email)
                .ToListAsync();

            Console.WriteLine($"Found {managedStaff.Count} staff managed by Oliver");

            // Create PDRs for staff Oliver manages
            foreach (var staff in managedStaff)
            {
                Console.WriteLine($"Creating PDR for {staff.FirstName} {staff.LastName}");
                
                // Mix of statuses for managed staff
                if (staff.FirstName == "Rachel")
                {
                    await CreatePDRWithStatus(context, staff.StaffReference, 2024, PDRStatus.StaffCompleted, questions);
                    await CreatePDRWithStatus(context, staff.StaffReference, 2023, PDRStatus.Completed, questions);
                }
                else if (staff.FirstName == "David")
                {
                    await CreatePDRWithStatus(context, staff.StaffReference, 2024, PDRStatus.ReadyForCollaboration, questions);
                }
                else if (staff.FirstName == "Lisa")
                {
                    await CreatePDRWithStatus(context, staff.StaffReference, 2024, PDRStatus.StaffCompleted, questions);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Test PDRs created successfully!");
        }

        private static async Task CreatePDRWithStatus(
            ApplicationDbContext context, 
            string staffReference, 
            int year, 
            PDRStatus status, 
            List<PDRQuestion> questions)
        {
            var pdr = new PDR
            {
                StaffReference = staffReference,
                Year = year,
                Month = DateTime.Now.Month, // Set current month
                Status = status,
                AssignedDate = DateTime.UtcNow.AddDays(-30),
                DueDate = DateTime.UtcNow.AddDays(30),
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };

            // Set completion dates based on status
            if (status >= PDRStatus.StaffCompleted)
            {
                pdr.StaffCompletedDate = DateTime.UtcNow.AddDays(-20);
                
                // Add staff responses
                foreach (var question in questions)
                {
                    pdr.Responses.Add(new PDRResponse
                    {
                        QuestionId = question.Id,
                        ResponseType = PDRType.Staff,
                        Response = $"Staff response for {question.Category} - {question.StaffQuestionText.Substring(0, Math.Min(50, question.StaffQuestionText.Length))}...",
                        Rating = Random.Shared.Next(3, 6),
                        ResponseDate = pdr.StaffCompletedDate.Value
                    });
                }
            }

            if (status >= PDRStatus.ReadyForCollaboration)
            {
                pdr.ManagerCompletedDate = DateTime.UtcNow.AddDays(-10);
                
                // Add manager responses for questions that have manager components
                foreach (var question in questions.Where(q => q.HasManagerQuestion))
                {
                    pdr.Responses.Add(new PDRResponse
                    {
                        QuestionId = question.Id,
                        ResponseType = PDRType.Manager,
                        Response = $"Manager response for {question.Category} - {question.ManagerQuestionText?.Substring(0, Math.Min(50, question.ManagerQuestionText.Length))}...",
                        Rating = Random.Shared.Next(3, 6),
                        ResponseDate = pdr.ManagerCompletedDate.Value
                    });
                }
            }

            if (status == PDRStatus.Completed)
            {
                pdr.CollaborativeCompletedDate = DateTime.UtcNow.AddDays(-5);
            }

            context.PDRs.Add(pdr);
            await context.SaveChangesAsync();

            // Create comparisons if ready for collaboration or completed
            if (status >= PDRStatus.ReadyForCollaboration)
            {
                foreach (var question in questions)
                {
                    var staffResponse = pdr.Responses.FirstOrDefault(r => r.QuestionId == question.Id && r.ResponseType == PDRType.Staff);
                    var managerResponse = pdr.Responses.FirstOrDefault(r => r.QuestionId == question.Id && r.ResponseType == PDRType.Manager);

                    var comparison = new PDRComparison
                    {
                        PDRId = pdr.Id,
                        QuestionId = question.Id,
                        StaffResponse = staffResponse?.Response,
                        StaffRating = staffResponse?.Rating,
                        ManagerResponse = managerResponse?.Response,
                        ManagerRating = managerResponse?.Rating,
                        CreatedDate = DateTime.UtcNow.AddDays(-10)
                    };

                    if (status == PDRStatus.Completed)
                    {
                        comparison.CollaborativeResponse = $"Agreed collaborative response for {question.Category} - Both parties agree on the assessment.";
                        comparison.CollaborativeRating = 4;
                    }

                    context.PDRComparisons.Add(comparison);
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
}