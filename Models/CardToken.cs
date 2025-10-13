using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoPoint.Models
{
    /// <summary>
    /// Represents a staff member's card token from the ulive database.
    /// Staff can have multiple card tokens.
    /// Reference is stored as hexadecimal string in the database.
    /// </summary>
    [Table("bc_v_net2_card_tokens")]
    public class CardToken
    {
        [Key]
        [Column("Reference")]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Column("CardNumber")]
        public int CardNumber { get; set; }

        // Convert hex Reference to integer employee number
        [NotMapped]
        public int EmployeeNumber
        {
            get
            {
                if (string.IsNullOrEmpty(Reference))
                    return 0;

                try
                {
                    // Convert hex string to integer (e.g., "7B583339" -> employee number)
                    return Convert.ToInt32(Reference, 16);
                }
                catch
                {
                    return 0;
                }
            }
        }

        // Not mapped - for display purposes
        [NotMapped]
        public string FormattedCardNumber => CardNumber.ToString("D8");
    }
}
