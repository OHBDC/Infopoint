using Microsoft.EntityFrameworkCore;
using InfoPoint.Models;

namespace InfoPoint.Data
{
    /// <summary>
    /// Database context for the Ulive database (card tokens, etc.)
    /// This is a read-only context for external data.
    /// </summary>
    public class UliveDbContext : DbContext
    {
        public UliveDbContext(DbContextOptions<UliveDbContext> options)
            : base(options)
        {
        }

        public DbSet<CardToken> CardTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure CardToken entity (existing view/table - read-only)
            builder.Entity<CardToken>(entity =>
            {
                entity.ToTable("bc_v_net2_card_tokens", t => t.ExcludeFromMigrations());
                // Composite key since staff can have multiple cards
                // Reference is a hex string (e.g., "55D9E2E" for employee 90002958)
                entity.HasKey(e => new { e.Reference, e.CardNumber });
                entity.Property(e => e.Reference).IsRequired().HasMaxLength(50);
            });
        }
    }
}
