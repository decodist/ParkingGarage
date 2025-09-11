namespace DemoAppDotNet.DTOs
{
    public class FloorDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int BuildingId { get; set; }
        public List<BayDto> Bays { get; set; } = new List<BayDto>();
    }
}