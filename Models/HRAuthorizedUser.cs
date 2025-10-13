using System.ComponentModel.DataAnnotations;

namespace InfoPoint.Models
{
    /// <summary>
    /// Represents users authorized to access the HR management system.
    /// Only users in this table can view/edit staff HR data.
    /// </summary>
    public class HRAuthorizedUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Created By")]
        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}
