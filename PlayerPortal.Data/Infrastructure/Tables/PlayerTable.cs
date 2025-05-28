using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerPortal.Data.Infrastructure.Tables
{
    [Table("Players")]
    public class PlayerTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 99)]
        public int ShirtNo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Appearance { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Goals { get; set; }

        public int? CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}

