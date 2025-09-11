using DemoAppDotNet.Classes;
using DemoAppDotNet.Models;
using DemoAppDotNet.Services;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // API support

// Add app-specific services
builder.Services.AddScoped<BuildingService>();
builder.Services.AddScoped<FloorService>();
builder.Services.AddScoped<SpotService>();
builder.Services.AddScoped<BayService>();
builder.Services.AddScoped<CarService>();

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
    
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Auto-create database and seed data
static async Task SeedDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Check if database is properly seeded by verifying core data exists
    var isSeeded = await context.Set<Building>().AnyAsync() &&
                   await context.Set<Floor>().AnyAsync() &&
                   await context.Set<Spot>().AnyAsync();

    if (!isSeeded)
    {
        // Clear any partial data first
        context.Set<Car>().RemoveRange(context.Set<Car>());
        context.Set<Rate>().RemoveRange(context.Set<Rate>());
        context.Set<Spot>().RemoveRange(context.Set<Spot>());
        context.Set<Bay>().RemoveRange(context.Set<Bay>());
        context.Set<Floor>().RemoveRange(context.Set<Floor>());
        context.Set<Building>().RemoveRange(context.Set<Building>());
        await context.SaveChangesAsync();

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
                    BuildingId = floor.BuildingId,
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
                var spotName = $"{floor.Number}{bayLetter}{spotNum:D2}";

                var spot = new Spot
                {
                    Number = spotNum,
                    Name = spotName,
                    Status = "Available",
                    FloorId = bay.FloorId,
                    BayId = bay.Id,
                    BuildingId = bay.BuildingId,
                    Meta =
                        $"{{\"type\":\"{new[] { "standard", "premium", "accessible" }[new Random().Next(3)]}\",\"size\":\"{new[] { "compact", "standard", "large" }[new Random().Next(3)]}\",\"ev_charging\":{(new Random().Next(2) == 1).ToString().ToLower()}}}"
                };
                await context.AddAsync(spot);
            }
        }

        await context.SaveChangesAsync();

        // Create some prices
        var spots = await context.Set<Spot>().Take(30).ToListAsync();
        foreach (var spot in spots)
        {
            var rate = new Rate
            {
                Day = "All",
                MinuteRate = (decimal)(2.0 / 60.0), // $2 per hour
                PremiumEv = 0.2m,
                SpotId = spot.Id
            };
            context.Set<Rate>().Add(rate);
        }

        // Add some cars
        var randomSpots = await context.Set<Spot>().OrderBy(x => Guid.NewGuid()).Take(45).ToListAsync();
        var samplePlates = new[] { "ABC-1234", "XYZ-5678", "DEF-9012", "GHI-3456", "JKL-7890" };

        for (int i = 0; i < randomSpots.Count; i++)
        {
            var spot = randomSpots[i];
            var shouldPark = i < randomSpots.Count * 0.7; // Park 70% of cars

            if (shouldPark)
            {
                spot.Status = "Occupied";
            }

            var car = new Car
            {
                Plate = $"{samplePlates[i % samplePlates.Length].Split('-')[0]}-{1000 + i}",
                CheckIn = shouldPark ? DateTime.UtcNow.AddHours(-new Random().Next(1, 8)) : DateTime.MinValue,
                Size = new[] { "Compact", "Standard", "Large" }[i % 3],
                SpotId = shouldPark ? spot.Id : null,
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

await SeedDatabaseAsync(app.Services);

app.Run();