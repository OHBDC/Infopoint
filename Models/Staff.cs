using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    [Table("Staff")]
    public class Staff
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Firstname")]
        [StringLength(100)]
        public string? Firstname { get; set; }

        [Column("Surname")]
        [StringLength(100)]
        public string? Surname { get; set; }

        [Column("Email")]
        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [Column("Manager")]
        [StringLength(200)]
        public string? Manager { get; set; }

        [Column("Department")]
        [StringLength(200)]
        public string? Department { get; set; }

        [Column("JobTitle")]
        [StringLength(200)]
        public string? JobTitle { get; set; }

        [Column("Active")]
        public int? Active { get; set; }

        [Column("Noupdate")]
        public int? Noupdate { get; set; }

        // Computed/Mapped properties for compatibility with existing code
        [NotMapped]
        public string FirstName
        {
            get => Firstname ?? "";
            set => Firstname = value;
        }

        [NotMapped]
        public string LastName
        {
            get => Surname ?? "";
            set => Surname = value;
        }

        [NotMapped]
        public string Area
        {
            get => Department ?? "";
            set => Department = value;
        }

        [NotMapped]
        public string? ManagerEmail
        {
            get => Manager;
            set => Manager = value;
        }

        [NotMapped]
        public string StaffReference
        {
            get => Id.ToString().PadLeft(8, '0');
            set { } // Empty setter for EF compatibility
        }

        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName => $"{Firstname} {Surname}".Trim();

        [NotMapped]
        public bool IsActive
        {
            get => (Active ?? 0) != 0;
            set => Active = value ? 1 : 0;
        }

        [NotMapped]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime? LastUpdated { get; set; }

        // Google email computed property (@g.bdc.ac.uk)
        [NotMapped]
        public string? GoogleEmail
        {
            get
            {
                if (string.IsNullOrEmpty(Email)) return null;
                // Convert @bdc.ac.uk to @g.bdc.ac.uk
                if (Email.EndsWith("@bdc.ac.uk", StringComparison.OrdinalIgnoreCase))
                {
                    return Email.Replace("@bdc.ac.uk", "@g.bdc.ac.uk", StringComparison.OrdinalIgnoreCase);
                }
                return Email;
            }
        }

        public override string ToString()
        {
            return $"{FullName} - {JobTitle}";
        }
    }
}
