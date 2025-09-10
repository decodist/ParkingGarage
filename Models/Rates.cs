using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Rates")]
    public class Rate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Day { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinuteRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PremiumEv { get; set; }

        [Required]
        public int SpotId { get; set; }

        public virtual Spot? Spot { get; set; }

        [StringLength(1000)]
        public string? Meta { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}