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
        public DbSet<PDR> PDRs { get; set; }
        public DbSet<PDRQuestion> PDRQuestions { get; set; }
        public DbSet<PDRResponse> PDRResponses { get; set; }
        public DbSet<PDRComparison> PDRComparisons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Staff entity
            builder.Entity<Staff>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.StaffReference).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StaffReference).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Area).IsRequired().HasMaxLength(100);
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ManagerEmail).HasMaxLength(100);
            });

            // Configure PDR entity
            builder.Entity<PDR>(entity =>
            {
                entity.HasOne(e => e.Staff)
                    .WithMany()
                    .HasForeignKey(e => e.StaffReference)
                    .HasPrincipalKey(s => s.StaffReference)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.StaffReference, e.Year }).IsUnique();
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
        }
    }
}