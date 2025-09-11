namespace DemoAppDotNet.DTOs
{
    public class BuildingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Geolocation { get; set; }
        public string? Meta { get; set; }
        public List<FloorDto> Floors { get; set; } = new();
    }
}