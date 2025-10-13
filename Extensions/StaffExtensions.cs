using InfoPoint.Data;
using InfoPoint.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoPoint.Extensions
{
    public static class StaffExtensions
    {
        /// <summary>
        /// Finds a staff member by email, checking both @bdc.ac.uk and @g.bdc.ac.uk formats
        /// </summary>
        public static async Task<Staff?> FindByEmailAsync(this DbSet<Staff> staff, string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            // Try direct match first
            var directMatch = await staff.FirstOrDefaultAsync(s => s.Email == email);
            if (directMatch != null) return directMatch;

            // If email is @g.bdc.ac.uk, try @bdc.ac.uk
            if (email.EndsWith("@g.bdc.ac.uk", StringComparison.OrdinalIgnoreCase))
            {
                var bdcEmail = email.Replace("@g.bdc.ac.uk", "@bdc.ac.uk", StringComparison.OrdinalIgnoreCase);
                return await staff.FirstOrDefaultAsync(s => s.Email == bdcEmail);
            }

            // If email is @bdc.ac.uk, try @g.bdc.ac.uk
            if (email.EndsWith("@bdc.ac.uk", StringComparison.OrdinalIgnoreCase))
            {
                var googleEmail = email.Replace("@bdc.ac.uk", "@g.bdc.ac.uk", StringComparison.OrdinalIgnoreCase);
                return await staff.FirstOrDefaultAsync(s => s.Email == googleEmail);
            }

            return null;
        }

        /// <summary>
        /// Converts a Google email (@g.bdc.ac.uk) to BDC email (@bdc.ac.uk)
        /// </summary>
        public static string? ToBdcEmail(this string? email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            if (email.EndsWith("@g.bdc.ac.uk", StringComparison.OrdinalIgnoreCase))
            {
                return email.Replace("@g.bdc.ac.uk", "@bdc.ac.uk", StringComparison.OrdinalIgnoreCase);
            }
            return email;
        }

        /// <summary>
        /// Converts a BDC email (@bdc.ac.uk) to Google email (@g.bdc.ac.uk)
        /// </summary>
        public static string? ToGoogleEmail(this string? email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            if (email.EndsWith("@bdc.ac.uk", StringComparison.OrdinalIgnoreCase))
            {
                return email.Replace("@bdc.ac.uk", "@g.bdc.ac.uk", StringComparison.OrdinalIgnoreCase);
            }
            return email;
        }
    }
}
