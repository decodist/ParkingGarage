using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Bays")]
    public class Bay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }
        
        public int FloorId { get; set; }
        public virtual Floor? Floor { get; set; }
        
        public int BuildingId { get; set; }
        public virtual Building? Building { get; set; }
        
        public virtual ICollection<Spot> Spots { get; set; } = new List<Spot>();

        [StringLength(1000)]
        public string? Meta { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}