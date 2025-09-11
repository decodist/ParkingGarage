namespace DemoAppDotNet.DTOs;

public class SpotDto
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string? Name { get; set; }
    public int FloorId { get; set; }
    public int BayId { get; set; }
    public int BuildingId { get; set; }
    public decimal MinuteRate { get; set; }
    public string? Status { get; set; }
    public string? Meta { get; set; }
}