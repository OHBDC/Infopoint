using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(8, MinimumLength = 8)]
        [Display(Name = "Staff Reference")]
        public string StaffReference { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Manager Email")]
        public string? ManagerEmail { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        public override string ToString()
        {
            return $"{FullName} ({StaffReference}) - {JobTitle}";
        }
    }
}