using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    public enum PDRStatus
    {
        Assigned,
        StaffCompleted,
        ManagerCompleted,
        ReadyForCollaboration,
        Completed
    }

    public enum PDRType
    {
        Staff,
        Manager,
        Collaborative
    }

    public class PDR
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(8)]
        public string StaffReference { get; set; } = string.Empty;

        [ForeignKey("StaffReference")]
        public Staff Staff { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        [StringLength(50)]
        public string? Period { get; set; } // e.g., "Q1", "March", "Mid-Year Review"

        [Required]
        public PDRStatus Status { get; set; } = PDRStatus.Assigned;

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? StaffCompletedDate { get; set; }

        public DateTime? ManagerCompletedDate { get; set; }

        public DateTime? CollaborativeCompletedDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        public ICollection<PDRResponse> Responses { get; set; } = new List<PDRResponse>();
    }

    public class PDRQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string StaffQuestionText { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ManagerQuestionText { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int Order { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool HasManagerQuestion { get; set; } = false;

        [StringLength(1000)]
        public string? Description { get; set; }

        public ICollection<PDRResponse> Responses { get; set; } = new List<PDRResponse>();
    }

    public class PDRResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PDRId { get; set; }

        [ForeignKey("PDRId")]
        public PDR PDR { get; set; } = null!;

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public PDRQuestion Question { get; set; } = null!;

        [Required]
        public PDRType ResponseType { get; set; }

        [Required]
        [Column(TypeName = "TEXT")]
        public string Response { get; set; } = string.Empty;

        public int? Rating { get; set; }

        [Required]
        public DateTime ResponseDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class PDRComparison
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PDRId { get; set; }

        [ForeignKey("PDRId")]
        public PDR PDR { get; set; } = null!;

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public PDRQuestion Question { get; set; } = null!;

        [Column(TypeName = "TEXT")]
        public string? StaffResponse { get; set; }

        [Column(TypeName = "TEXT")]
        public string? ManagerResponse { get; set; }

        public int? StaffRating { get; set; }

        public int? ManagerRating { get; set; }

        [Column(TypeName = "TEXT")]
        public string? CollaborativeResponse { get; set; }

        public int? CollaborativeRating { get; set; }

        [StringLength(500)]
        public string? ComparisonNotes { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}