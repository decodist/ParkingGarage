namespace DemoAppDotNet.DTOs
{
    public class BayDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public List<SpotDto> Spots { get; set; } = new List<SpotDto>();
    }
}