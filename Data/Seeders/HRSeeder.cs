using InfoPoint.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoPoint.Data.Seeders
{
    public static class HRSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed HR Authorized Users
            await SeedHRAuthorizedUsersAsync(context);
        }

        private static async Task SeedHRAuthorizedUsersAsync(ApplicationDbContext context)
        {
            // Check if oliver.hill@g.bdc.ac.uk already exists
            var exists = await context.HRAuthorizedUsers
                .AnyAsync(u => u.Email == "oliver.hill@g.bdc.ac.uk");

            if (!exists)
            {
                var hrUser = new HRAuthorizedUser
                {
                    Email = "oliver.hill@g.bdc.ac.uk",
                    FullName = "Oliver Hill",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System Seed"
                };

                context.HRAuthorizedUsers.Add(hrUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
