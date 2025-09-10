using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Floors")]
    public class Floor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        public int BuildingId { get; set; }
        
        public virtual Building? Building { get; set; }

        public virtual ICollection<Bay> Bays { get; set; } = new List<Bay>();
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
        
        [StringLength(1000)]
        public string? Meta { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}