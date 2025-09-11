using DemoAppDotNet.Classes;
using DemoAppDotNet.DTOs;
using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoAppDotNet.Services
{
    public class FloorService
    {
        private readonly ApplicationDbContext _context;

        public FloorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FloorDto>> GetAllFloorsAsync()
        {
            var floors = await _context.Floors
                .Include(f => f.Bays)
                .ThenInclude(b => b.Spots)
                .ToListAsync();
            
            return floors.Select(f => new FloorDto
            {
                Id = f.Id,
                Number = f.Number,
                BuildingId = f.BuildingId,
                Bays = f.Bays.Select(b => new BayDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Location = b.Location,
                    FloorId = b.FloorId,
                    BuildingId = b.BuildingId,
                    Spots = b.Spots.Select(s => new SpotDto
                    {
                        Id = s.Id,
                        Number = s.Number
                    }).ToList()
                }).ToList()
            });
        }

        public async Task<FloorDto?> GetFloorByIdAsync(int id)
        {
            var floor = await _context.Floors
                .Include(f => f.Bays)
                .ThenInclude(b => b.Spots)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (floor == null) return null;

            return new FloorDto
            {
                Id = floor.Id,
                Number = floor.Number,
                BuildingId = floor.BuildingId,
                Bays = floor.Bays.Select(b => new BayDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Location = b.Location,
                    FloorId = b.FloorId,
                    BuildingId = b.BuildingId,
                    Spots = b.Spots.Select(s => new SpotDto
                    {
                        Id = s.Id,
                        Number = s.Number
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<FloorDto> CreateFloorAsync(Floor floor)
        {
            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();

            return new FloorDto
            {
                Id = floor.Id,
                Number = floor.Number,
                BuildingId = floor.BuildingId,
                Bays = new List<BayDto>()
            };
        }

        public async Task<FloorDto?> UpdateFloorAsync(int id, Floor updatedFloor)
        {
            var floor = await _context.Floors.FindAsync(id);
            if (floor == null) return null;

            floor.Number = updatedFloor.Number;
            floor.BuildingId = updatedFloor.BuildingId;
            await _context.SaveChangesAsync();

            return new FloorDto
            {
                Id = floor.Id,
                Number = floor.Number,
                BuildingId = floor.BuildingId,
                Bays = new List<BayDto>()
            };
        }

        public async Task<bool> DeleteFloorAsync(int id)
        {
            var floor = await _context.Floors.FindAsync(id);
            if (floor == null) return false;

            _context.Floors.Remove(floor);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}