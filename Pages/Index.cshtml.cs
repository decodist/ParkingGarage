using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DemoAppDotNet.Classes;
using DemoAppDotNet.Models;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace DemoAppDotNet.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Properties to hold data for the view
    public List<Building> Buildings { get; set; } = new();
    public List<Floor> Floors { get; set; } = new();
    public List<Bay> Bays { get; set; } = new();
    public List<Spot> Spots { get; set; } = new();
    public List<Car> Cars { get; set; } = new();
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
    public int OccupiedSpots { get; set; }

    public async Task OnGet()
    {
        // Get all parking-related data
        Buildings = await _context.GetAllAsync<Building>();
        Floors = await _context.GetAllAsync<Floor>();
        Bays = await _context.GetAllAsync<Bay>();
        Spots = await _context.GetAllAsync<Spot>();
        Cars = await _context.GetAllAsync<Car>();
        
        // Calculate statistics
        TotalSpots = await _context.CountAsync<Spot>();
        
        // Count available spots (assuming status field exists)
        AvailableSpots = Spots.Count(s => s.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));
        
        // Count occupied spots
        OccupiedSpots = Cars.Count(c => c.CheckOut == null); // Cars that haven't checked out
    }
}