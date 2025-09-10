using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DemoAppDotNet.Controllers;
using DemoAppDotNet.Classes;
using DemoAppDotNet.Models;

namespace DemoAppDotNet.Tests
{
    public class ParkingGarageDbTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB name per test
                .Options;
        
            return new ApplicationDbContext(options);
        }

        [Fact(Skip = "Temporarily disabled")]
        public async Task DatabaseTables_AllParkingTablesExist_ReturnsSuccess()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            // Create the database schema in memory
            await context.Database.EnsureCreatedAsync();
        
            // Act & Assert
            var buildingExists = await context.ExistsAsync<Building>(b => true);
            
            // Should not throw an exception - tables exist but are empty
            Assert.False(buildingExists);
        }

        [Fact(Skip = "Temporarily disabled")]
        public async Task SeedData_CreatesParkingStructure_ReturnsCorrectCounts()
        {
            // Arrange
            using var context = GetInMemoryContext();
            await context.Database.EnsureCreatedAsync();

            // Act - Create the parking structure
            var building = new Building { Meta = "{\"type\":\"multi-level\"}" };
            await context.AddAsync(building);
            await context.SaveChangesAsync();

            // Create 5 floors
            for (int floorNum = 1; floorNum <= 5; floorNum++)
            {
                var floor = new Floor
                {
                    Number = floorNum,
                    BuildingId = building.Id,
                    Meta = $"{{\"elevation\":{floorNum * 3}}}"
                };
                await context.AddAsync(floor);
            }
            await context.SaveChangesAsync();

            // Create 3 bays per floor (15 total)
            var floors = await context.Set<Floor>().ToListAsync();
            foreach (var floor in floors)
            {
                for (int bayIndex = 0; bayIndex < 3; bayIndex++)
                {
                    var bay = new Bay
                    {
                        FloorId = floor.Id,
                        Meta = $"{{\"section\":\"{(char)('A' + bayIndex)}\"}}"
                    };
                    await context.AddAsync(bay);
                }
            }
            await context.SaveChangesAsync();

            // Create 10 spots per bay (150 total)
            var bays = await context.Set<Bay>().ToListAsync();
            foreach (var bay in bays)
            {
                for (int spotNum = 1; spotNum <= 10; spotNum++)
                {
                    var floor = floors.First(f => f.Id == bay.FloorId);
                    var spotNumber = $"{floor.Number}{(char)('A' + (bays.IndexOf(bay) % 3))}{spotNum:D2}";
                    
                    var spot = new Spot
                    {
                        Number = spotNumber,
                        Status = "Available",
                        FloorId = bay.FloorId,
                        BayId = bay.Id,
                        Meta = "{\"type\":\"standard\"}"
                    };
                    await context.AddAsync(spot);
                }
            }
            await context.SaveChangesAsync();

            // Add sample rates
            var firstSpot = await context.Set<Spot>().FirstAsync();
            var rate = new Rate
            {
                Day = "Monday-Friday",
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(18),
                MinuteRate = 0.25m,
                PremiumEv = 0.10m,
                SpotId = firstSpot.Id
            };
            await context.AddAsync(rate);

            // Add sample car
            var car = new Car
            {
                Plate = "ABC-1234",
                CheckIn = DateTime.UtcNow,
                Size = "Standard",
                SpotId = firstSpot.Id,
                Meta = "{\"color\":\"blue\"}"
            };
            await context.AddAsync(car);
            await context.SaveChangesAsync();

            // Assert - Verify correct structure
            Assert.Equal(1, await context.Set<Building>().CountAsync());
            Assert.Equal(5, await context.Set<Floor>().CountAsync());
            Assert.Equal(15, await context.Set<Bay>().CountAsync()); // 5 floors * 3 bays
            Assert.Equal(150, await context.Set<Spot>().CountAsync()); // 15 bays * 10 spots
            Assert.Equal(1, await context.Set<Rate>().CountAsync());
            Assert.Equal(1, await context.Set<Car>().CountAsync());
        }

        [Fact(Skip = "Temporarily disabled")]
        public async Task Relationships_ForeignKeysWork_ReturnsValidData()
        {
            // Arrange
            using var context = GetInMemoryContext();
            await context.Database.EnsureCreatedAsync();

            // Act - Create related entities
            var building = new Building { Meta = "{}" };
            await context.AddAsync(building);
            await context.SaveChangesAsync();

            var floor = new Floor { Number = 1, BuildingId = building.Id, Meta = "{}" };
            await context.AddAsync(floor);
            await context.SaveChangesAsync();

            var bay = new Bay { FloorId = floor.Id, Meta = "{}" };
            await context.AddAsync(bay);
            await context.SaveChangesAsync();

            var spot = new Spot
            {
                Number = "1A01",
                Status = "Available",
                FloorId = floor.Id,
                BayId = bay.Id,
                Meta = "{}"
            };
            await context.AddAsync(spot);
            await context.SaveChangesAsync();

            // Assert - Verify relationships
            var retrievedSpot = await context.Set<Spot>()
                .Include(s => s.Floor)
                .Include(s => s.Bay)
                .FirstAsync();

            Assert.Equal(floor.Id, retrievedSpot.FloorId);
            Assert.Equal(bay.Id, retrievedSpot.BayId);
            Assert.Equal(1, retrievedSpot.Floor.Number);
        }
    }
}