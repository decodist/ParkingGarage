using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoAppDotNet.Models
{
    [Table("Buildings")]
    public class Building
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Geolocation { get; set; }

        [StringLength(1000)]
        public string? Meta { get; set; }

        public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}