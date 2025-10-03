using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    public class SmartTarget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PDRId { get; set; }

        [ForeignKey("PDRId")]
        public PDR PDR { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public string? Description { get; set; }

        [Required]
        public bool Specific { get; set; } = false;

        [Required]
        public bool Measurable { get; set; } = false;

        [Required]
        public bool Achievable { get; set; } = false;

        [Required]
        public bool Relevant { get; set; } = false;

        [Required]
        public bool TimeBound { get; set; } = false;

        [Required]
        public DateTime TargetDate { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; } // High, Medium, Low

        [StringLength(100)]
        public string? Category { get; set; } // Performance, Development, Wellbeing, etc.

        [StringLength(50)]
        public string? Status { get; set; } = "Pending"; // Pending, In Progress, Completed

        [Column(TypeName = "TEXT")]
        public string? SuccessCriteria { get; set; }

        [Column(TypeName = "TEXT")]
        public string? ActionPlan { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        [Required]
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}