using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    /// <summary>
    /// Represents the HR organizational data for staff members.
    /// This is the source of truth for organizational hierarchy and reporting structure.
    /// Links to Staff table via EmployeeNumber (Staff.Id)
    /// </summary>
    public class StaffHR
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee Number")]
        public int EmployeeNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Hierarchy Level 1")]
        public string? HierarchyLevel1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Hierarchy Level 2")]
        public string? HierarchyLevel2 { get; set; }

        [StringLength(200)]
        [Display(Name = "Hierarchy Level 3")]
        public string? HierarchyLevel3 { get; set; }

        [StringLength(200)]
        [Display(Name = "Hierarchy Level 4")]
        public string? HierarchyLevel4 { get; set; }

        [StringLength(200)]
        [Display(Name = "Hierarchy Level 5")]
        public string? HierarchyLevel5 { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Contract Type")]
        public string ContractType { get; set; } = string.Empty;

        [Display(Name = "Manager Employee Number")]
        public int? ManagerEmployeeNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Manager Name")]
        public string? ManagerName { get; set; }

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        // Navigation property to link with Staff table from Active Directory
        [ForeignKey("EmployeeNumber")]
        public Staff? StaffAD { get; set; }

        // Navigation property for manager's AD data (not mapped to database)
        [NotMapped]
        public Staff? ManagerAD { get; set; }

        // Navigation property for card tokens (not mapped to database - loaded from Ulive DB)
        [NotMapped]
        public List<CardToken> CardTokens { get; set; } = new List<CardToken>();
    }
}
