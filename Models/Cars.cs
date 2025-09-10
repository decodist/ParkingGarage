using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Cars")]
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Plate { get; set; } = string.Empty;

        [Required]
        public DateTime CheckIn { get; set; } = DateTime.UtcNow;

        public DateTime? CheckOut { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        public int SpotId { get; set; }
        public virtual Spot Spot { get; set; } = null!;
        
        [StringLength(1000)]
        public string? Meta { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}