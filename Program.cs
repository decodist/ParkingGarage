using DemoAppDotNet.Classes;
using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // API support

// Configure Kestrel to listen on HTTP only
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005); // HTTP only
});

// DB with environment variables
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Environment.EnvironmentName switch
    {
        "Development" => $"Server=localhost,1433;Database=DemoAppDb;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};TrustServerCertificate=true;",
        "Production" => $"Server=tcp:demo-app-dot-net-database-server.database.windows.net,1433;Initial Catalog=demo-app-dot-net-database;Persist Security Info=False;User ID={Environment.GetEnvironmentVariable("PROD_DB_USER")};Password={Environment.GetEnvironmentVariable("PROD_DB_PASSWORD")};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
        _ => $"Server=localhost,1433;Database=DemoAppDb;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};TrustServerCertificate=true;",
    };
    Console.WriteLine(connectionString);
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Auto-create database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed initial data if tables are empty
    if (!await context.Set<Building>().AnyAsync())
    {
        // Create main building
        var building = new Building
        {
            Name = "Main Parking Building",
            Address = "123 Main Street",
            Meta = "{\"type\":\"multi-level\",\"security\":\"24/7\"}"
        };
        await context.AddAsync(building);
        await context.SaveChangesAsync();

        // Create 5 floors
        for (int floorNum = 1; floorNum <= 5; floorNum++)
        {
            var floor = new Floor
            {
                Number = floorNum,
                BuildingId = building.Id,
                Meta = $"{{\"elevation\":{floorNum * 3},\"access\":\"elevator\"}}"
            };
            await context.AddAsync(floor);
        }
        await context.SaveChangesAsync();

        // Create 3 bays per floor
        var floors = await context.Set<Floor>().ToListAsync();
        foreach (var floor in floors)
        {
            var bayNames = new[] { "A", "B", "C" };
            for (int bayIndex = 0; bayIndex < 3; bayIndex++)
            {
                var bay = new Bay
                {
                    Name = $"Bay {bayNames[bayIndex]}",
                    FloorId = floor.Id,
                    Meta = $"{{\"section\":\"{bayNames[bayIndex]}\",\"capacity\":10}}"
                };
                await context.AddAsync(bay);
            }
        }
        await context.SaveChangesAsync();

        // Create 10 spots per bay
        var bays = await context.Set<Bay>().ToListAsync();
        foreach (var bay in bays)
        {
            for (int spotNum = 1; spotNum <= 10; spotNum++)
            {
                var floor = floors.First(f => f.Id == bay.FloorId);
                var bayLetter = bay.Name.Split(' ')[1];
                var spotNumber = $"{floor.Number}{bayLetter}{spotNum:D2}";
                
                var spot = new Spot
                {
                    Number = spotNumber,
                    Status = "Available",
                    FloorId = bay.FloorId,
                    BayId = bay.Id,
                    Meta = $"{{\"type\":\"standard\",\"size\":\"compact\",\"ev_charging\":false}}"
                };
                await context.AddAsync(spot);
            }
        }
        await context.SaveChangesAsync();

        // Create sample rates for spots
        var spots = await context.Set<Spot>().Take(30).ToListAsync();
        foreach (var spot in spots)
        {
            var rates = new[]
            {
                new Rate { Day = "Monday-Friday", StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(18), MinuteRate = 0.25m, PremiumEv = 0.10m, SpotId = spot.Id },
                new Rate { Day = "Monday-Friday", StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(8), MinuteRate = 0.15m, PremiumEv = 0.05m, SpotId = spot.Id },
                new Rate { Day = "Weekend", StartTime = TimeSpan.FromHours(0), EndTime = new TimeSpan(23, 59, 0), MinuteRate = 0.20m, PremiumEv = 0.08m, SpotId = spot.Id }
            };
            context.Set<Rate>().AddRange(rates);
        }

        // Add some sample cars
        var randomSpots = await context.Set<Spot>().OrderBy(x => Guid.NewGuid()).Take(45).ToListAsync();
        var samplePlates = new[] { "ABC-1234", "XYZ-5678", "DEF-9012", "GHI-3456", "JKL-7890" };
        
        for (int i = 0; i < randomSpots.Count; i++)
        {
            var spot = randomSpots[i];
            spot.Status = "Occupied";
            
            var car = new Car
            {
                Plate = $"{samplePlates[i % samplePlates.Length].Split('-')[0]}-{1000 + i}",
                CheckIn = DateTime.UtcNow.AddHours(-new Random().Next(1, 8)),
                Size = new[] { "Compact", "Standard", "Large" }[i % 3],
                SpotId = spot.Id,
                Meta = "{\"color\":\"blue\",\"model\":\"sedan\"}"
            };
            await context.AddAsync(car);
        }

        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // API controllers

app.Run();