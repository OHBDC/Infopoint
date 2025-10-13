using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InfoPoint.Models;

namespace InfoPoint.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffHR> StaffHR { get; set; }
        public DbSet<HRAuthorizedUser> HRAuthorizedUsers { get; set; }
        public DbSet<PDR> PDRs { get; set; }
        public DbSet<PDRQuestion> PDRQuestions { get; set; }
        public DbSet<PDRResponse> PDRResponses { get; set; }
        public DbSet<PDRComparison> PDRComparisons { get; set; }
        public DbSet<SmartTarget> SmartTargets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use custom schema for Identity tables to avoid conflicts
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("AspNetRoles", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("AspNetUserRoles", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "InfoPoint");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("AspNetUserTokens", "InfoPoint");

            // Configure Staff entity (existing table in SQL Server - don't manage schema)
            builder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staff", t => t.ExcludeFromMigrations());
                // Don't configure constraints - table already exists in database
            });

            // Configure PDR entity
            builder.Entity<PDR>(entity =>
            {
                // Ignore Staff navigation property - relationship cannot be configured as FK
                // StaffReference (string) in PDR doesn't match Staff.Id (int)
                // Staff data should be loaded manually when needed
                entity.Ignore(e => e.Staff);

                // Unique constraint: One PDR per staff member per year and period
                // This allows multiple PDRs per year (e.g., Oct-Dec, Feb-Mar, Jun-Jul)
                entity.HasIndex(e => new { e.StaffReference, e.Year, e.Period }).IsUnique();
                entity.Property(e => e.StaffReference).IsRequired().HasMaxLength(8);
            });

            // Configure PDRQuestion entity
            builder.Entity<PDRQuestion>(entity =>
            {
                entity.Property(e => e.StaffQuestionText).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ManagerQuestionText).HasMaxLength(500);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.HasManagerQuestion).IsRequired();
                entity.HasIndex(e => new { e.Category, e.Order });
            });

            // Configure PDRResponse entity
            builder.Entity<PDRResponse>(entity =>
            {
                entity.HasOne(e => e.PDR)
                    .WithMany(p => p.Responses)
                    .HasForeignKey(e => e.PDRId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.Responses)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Response).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.HasIndex(e => new { e.PDRId, e.QuestionId, e.ResponseType }).IsUnique();
            });

            // Configure PDRComparison entity
            builder.Entity<PDRComparison>(entity =>
            {
                entity.HasOne(e => e.PDR)
                    .WithMany()
                    .HasForeignKey(e => e.PDRId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany()
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.StaffResponse).HasColumnType("TEXT");
                entity.Property(e => e.ManagerResponse).HasColumnType("TEXT");
                entity.Property(e => e.CollaborativeResponse).HasColumnType("TEXT");
                entity.Property(e => e.ComparisonNotes).HasMaxLength(500);
                entity.HasIndex(e => new { e.PDRId, e.QuestionId }).IsUnique();
            });

            // Configure SmartTarget entity
            builder.Entity<SmartTarget>(entity =>
            {
                entity.HasOne(e => e.PDR)
                    .WithMany(p => p.SmartTargets)
                    .HasForeignKey(e => e.PDRId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.SuccessCriteria).HasColumnType("TEXT");
                entity.Property(e => e.ActionPlan).HasColumnType("TEXT");
                entity.HasIndex(e => new { e.PDRId, e.DisplayOrder });
            });

            // Configure StaffHR entity
            builder.Entity<StaffHR>(entity =>
            {
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.Property(e => e.EmployeeNumber).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.HierarchyLevel1).HasMaxLength(200);
                entity.Property(e => e.HierarchyLevel2).HasMaxLength(200);
                entity.Property(e => e.HierarchyLevel3).HasMaxLength(200);
                entity.Property(e => e.HierarchyLevel4).HasMaxLength(200);
                entity.Property(e => e.HierarchyLevel5).HasMaxLength(200);
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContractType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ManagerName).HasMaxLength(200);

                // Configure relationship with Staff table - make it optional (no FK constraint)
                entity.Ignore(e => e.StaffAD);
            });

            // Configure HRAuthorizedUser entity
            builder.Entity<HRAuthorizedUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
            });
        }
    }
}