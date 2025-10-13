using InfoPoint.Data;
using InfoPoint.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoPoint.Data.Seeders
{
    public static class PDRSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Only seed if no PDRs exist
            if (await context.PDRs.AnyAsync())
                return;

            // Get some staff members to create PDRs for
            var staffMembers = await context.Staff
                .Where(s => s.IsActive)
                .Take(5)
                .ToListAsync();

            if (!staffMembers.Any())
                return;

            var currentYear = DateTime.Now.Year;
            var random = new Random();

            var sampleStaffResponses = new[]
            {
                "I have consistently exceeded my targets this year and taken on additional responsibilities to support my team.",
                "I completed all assigned projects on time and received positive feedback from clients and colleagues.",
                "This year I focused on developing my technical skills and successfully implemented several process improvements.",
                "I maintained high performance standards while adapting to new systems and working practices.",
                "I have contributed to team success through collaboration and knowledge sharing with newer team members."
            };

            var sampleManagerResponses = new[]
            {
                "Employee has shown excellent performance and leadership qualities throughout the year.",
                "Consistently delivers high-quality work and demonstrates strong problem-solving abilities.",
                "Has taken initiative on several projects and shown great potential for career advancement.",
                "Reliable team member who contributes positively to team dynamics and client relationships.",
                "Shows strong commitment to professional development and helps mentor junior staff members."
            };

            var sampleCollaborativeResponses = new[]
            {
                "We agree that performance has been strong this year with notable achievements in project delivery and team collaboration.",
                "Both parties acknowledge the successful completion of key objectives and the positive impact on team performance.",
                "There is mutual agreement on the high standard of work delivered and the professional development achieved.",
                "We concur that the employee has demonstrated excellent skills and made valuable contributions to the organization.",
                "Both manager and staff member agree on the successful achievement of goals and positive trajectory for future development."
            };

            // Create PDRs for each staff member
            foreach (var staff in staffMembers)
            {
                // Create current year PDR
                var currentPDR = new PDR
                {
                    StaffReference = staff.StaffReference,
                    Year = currentYear,
                    Status = (PDRStatus)random.Next(0, 5), // Random status for variety
                    AssignedDate = DateTime.UtcNow.AddDays(-random.Next(30, 90)),
                    DueDate = DateTime.UtcNow.AddDays(random.Next(10, 60))
                };

                // Set completion dates based on status
                if (currentPDR.Status >= PDRStatus.StaffCompleted)
                {
                    currentPDR.StaffCompletedDate = currentPDR.AssignedDate.AddDays(random.Next(5, 20));
                }
                if (currentPDR.Status >= PDRStatus.ManagerCompleted)
                {
                    currentPDR.ManagerCompletedDate = currentPDR.StaffCompletedDate?.AddDays(random.Next(3, 10));
                }
                if (currentPDR.Status == PDRStatus.Completed)
                {
                    currentPDR.CollaborativeCompletedDate = currentPDR.ManagerCompletedDate?.AddDays(random.Next(1, 7));
                }

                context.PDRs.Add(currentPDR);
                await context.SaveChangesAsync(); // Save to get the ID

                // Create responses based on PDR status
                var questions = await context.PDRQuestions.ToListAsync();
                
                foreach (var question in questions)
                {
                    // Staff responses
                    if (currentPDR.Status >= PDRStatus.StaffCompleted)
                    {
                        var staffResponse = new PDRResponse
                        {
                            PDRId = currentPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Staff,
                            Response = sampleStaffResponses[random.Next(sampleStaffResponses.Length)],
                            Rating = random.Next(3, 6), // 3-5 rating
                            ResponseDate = currentPDR.StaffCompletedDate ?? DateTime.UtcNow
                        };
                        context.PDRResponses.Add(staffResponse);
                    }

                    // Manager responses
                    if (currentPDR.Status >= PDRStatus.ManagerCompleted)
                    {
                        var managerResponse = new PDRResponse
                        {
                            PDRId = currentPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Manager,
                            Response = sampleManagerResponses[random.Next(sampleManagerResponses.Length)],
                            Rating = random.Next(3, 6), // 3-5 rating
                            ResponseDate = currentPDR.ManagerCompletedDate ?? DateTime.UtcNow
                        };
                        context.PDRResponses.Add(managerResponse);
                    }

                    // Collaborative responses
                    if (currentPDR.Status == PDRStatus.Completed)
                    {
                        var collaborativeResponse = new PDRResponse
                        {
                            PDRId = currentPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Collaborative,
                            Response = sampleCollaborativeResponses[random.Next(sampleCollaborativeResponses.Length)],
                            Rating = random.Next(3, 6), // 3-5 rating
                            ResponseDate = currentPDR.CollaborativeCompletedDate ?? DateTime.UtcNow
                        };
                        context.PDRResponses.Add(collaborativeResponse);

                        // Create comparison record
                        var staffResp = await context.PDRResponses
                            .FirstOrDefaultAsync(r => r.PDRId == currentPDR.Id && r.QuestionId == question.Id && r.ResponseType == PDRType.Staff);
                        var managerResp = await context.PDRResponses
                            .FirstOrDefaultAsync(r => r.PDRId == currentPDR.Id && r.QuestionId == question.Id && r.ResponseType == PDRType.Manager);

                        var comparison = new PDRComparison
                        {
                            PDRId = currentPDR.Id,
                            QuestionId = question.Id,
                            StaffResponse = staffResp?.Response,
                            ManagerResponse = managerResp?.Response,
                            StaffRating = staffResp?.Rating,
                            ManagerRating = managerResp?.Rating,
                            CollaborativeResponse = collaborativeResponse.Response,
                            CollaborativeRating = collaborativeResponse.Rating,
                            ComparisonNotes = "Both parties reached mutual agreement on this assessment."
                        };
                        context.PDRComparisons.Add(comparison);
                    }
                }

                // Create previous year PDR (completed)
                if (random.Next(1, 3) == 1) // 50% chance
                {
                    var previousPDR = new PDR
                    {
                        StaffReference = staff.StaffReference,
                        Year = currentYear - 1,
                        Status = PDRStatus.Completed,
                        AssignedDate = DateTime.UtcNow.AddYears(-1).AddDays(-random.Next(30, 90)),
                        DueDate = DateTime.UtcNow.AddYears(-1).AddDays(30),
                        StaffCompletedDate = DateTime.UtcNow.AddYears(-1).AddDays(-random.Next(50, 70)),
                        ManagerCompletedDate = DateTime.UtcNow.AddYears(-1).AddDays(-random.Next(40, 60)),
                        CollaborativeCompletedDate = DateTime.UtcNow.AddYears(-1).AddDays(-random.Next(30, 50))
                    };

                    context.PDRs.Add(previousPDR);
                    await context.SaveChangesAsync(); // Save to get the ID

                    // Add responses for previous year PDR
                    foreach (var question in questions)
                    {
                        // Staff response
                        var staffResponse = new PDRResponse
                        {
                            PDRId = previousPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Staff,
                            Response = sampleStaffResponses[random.Next(sampleStaffResponses.Length)],
                            Rating = random.Next(2, 6), // 2-5 rating for historical data
                            ResponseDate = previousPDR.StaffCompletedDate.Value
                        };
                        context.PDRResponses.Add(staffResponse);

                        // Manager response
                        var managerResponse = new PDRResponse
                        {
                            PDRId = previousPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Manager,
                            Response = sampleManagerResponses[random.Next(sampleManagerResponses.Length)],
                            Rating = random.Next(2, 6), // 2-5 rating for historical data
                            ResponseDate = previousPDR.ManagerCompletedDate.Value
                        };
                        context.PDRResponses.Add(managerResponse);

                        // Collaborative response
                        var collaborativeResponse = new PDRResponse
                        {
                            PDRId = previousPDR.Id,
                            QuestionId = question.Id,
                            ResponseType = PDRType.Collaborative,
                            Response = sampleCollaborativeResponses[random.Next(sampleCollaborativeResponses.Length)],
                            Rating = random.Next(2, 6), // 2-5 rating for historical data
                            ResponseDate = previousPDR.CollaborativeCompletedDate.Value
                        };
                        context.PDRResponses.Add(collaborativeResponse);

                        // Create comparison
                        var comparison = new PDRComparison
                        {
                            PDRId = previousPDR.Id,
                            QuestionId = question.Id,
                            StaffResponse = staffResponse.Response,
                            ManagerResponse = managerResponse.Response,
                            StaffRating = staffResponse.Rating,
                            ManagerRating = managerResponse.Rating,
                            CollaborativeResponse = collaborativeResponse.Response,
                            CollaborativeRating = collaborativeResponse.Rating,
                            ComparisonNotes = "Historical PDR - completed successfully with mutual agreement."
                        };
                        context.PDRComparisons.Add(comparison);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}