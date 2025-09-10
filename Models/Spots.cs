using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Spots")]
    public class Spot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available";

        public int FloorId { get; set; }
        public virtual Floor Floor { get; set; } = null!;

        public int BayId { get; set; }
        public virtual Bay Bay { get; set; } = null!;

        [StringLength(2000)]
        public string? Meta { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}