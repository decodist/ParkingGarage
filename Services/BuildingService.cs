using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;
using DemoAppDotNet.Classes;
using DemoAppDotNet.DTOs;

namespace DemoAppDotNet.Services
{
    public class BuildingService
    {
        private readonly ApplicationDbContext _context;

        public BuildingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync()
        {
            var buildings = await _context.Buildings
                .Include(b => b.Floors)
                .ThenInclude(f => f.Bays)
                .ThenInclude(bay => bay.Spots)
                .ToListAsync();
        
            return buildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Floors = b.Floors.Select(f => new FloorDto
                {
                    Id = f.Id,
                    Number = f.Number,
                    BuildingId = f.BuildingId,
                    Bays = f.Bays.Select(bay => new BayDto
                    {
                        Id = bay.Id,
                        Name = bay.Name,
                        Location = bay.Location,
                        FloorId = bay.FloorId,
                        Spots = bay.Spots.Select(s => new SpotDto
                        {
                            Id = s.Id,
                            Number = s.Number,
                            Name = s.Name,
                            Meta = s.Meta
                        }).ToList()
                    }).ToList()
                }).ToList()
            });
        }
        
        public async Task<BuildingDto?> GetBuildingByIdAsync(int id)
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                .ThenInclude(f => f.Bays)
                .ThenInclude(bay => bay.Spots)
                .FirstOrDefaultAsync(b => b.Id == id);
        
            if (building == null) return null;
        
            return new BuildingDto
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                Floors = building.Floors.Select(f => new FloorDto
                {
                    Id = f.Id,
                    Number = f.Number,
                    BuildingId = f.BuildingId,
                    Bays = f.Bays.Select(bay => new BayDto
                    {
                        Id = bay.Id,
                        Name = bay.Name,
                        Location = bay.Location,
                        FloorId = f.Id,
                        BuildingId = bay.BuildingId,
                        Spots = bay.Spots.Select(s => new SpotDto
                        {
                            Id = s.Id,
                            Number = s.Number,
                            Name = s.Name,
                            FloorId = f.Id,
                            BayId = bay.Id,
                            BuildingId = s.BuildingId,
                            Status = s.Status,
                            Meta = s.Meta,
                            MinuteRate = _context.Rates
                                .Where(r => r.SpotId == s.Id)
                                .Select(r => r.MinuteRate)
                                .FirstOrDefault(),
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<Building> CreateBuildingAsync(Building building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task<Building?> UpdateBuildingAsync(int id, Building building)
        {
            var existingBuilding = await _context.Buildings.FindAsync(id);
            if (existingBuilding == null)
                return null;
            
            await _context.SaveChangesAsync();
            return existingBuilding;
        }

        public async Task<bool> DeleteBuildingAsync(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
                return false;

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}