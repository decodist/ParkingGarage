using DemoAppDotNet.Classes;
using DemoAppDotNet.DTOs;
using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoAppDotNet.Services
{
    public class BayService
    {
        private readonly ApplicationDbContext _context;

        public BayService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BayDto>> GetAllBaysAsync()
        {
            var bays = await _context.Bays
                .Include(b => b.Floor)
                .Include(b => b.Spots)
                .ToListAsync();

            return bays.Select(b => new BayDto
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
            });
        }

        public async Task<BayDto?> GetBayByIdAsync(int id)
        {
            var bay = await _context.Bays
                .Include(b => b.Floor)
                .Include(b => b.Spots)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bay == null) return null;

            return new BayDto
            {
                Id = bay.Id,
                Name = bay.Name,
                Location = bay.Location,
                FloorId = bay.FloorId,
                BuildingId = bay.BuildingId,
                Spots = bay.Spots.Select(s => new SpotDto
                {
                    Id = s.Id,
                    Number = s.Number
                }).ToList()
            };
        }

        public async Task<BayDto> CreateBayAsync(Bay bay)
        {
            _context.Bays.Add(bay);
            await _context.SaveChangesAsync();
        
            return new BayDto
            {
                Id = bay.Id,
                Name = bay.Name,
                Location = bay.Location,
                FloorId = bay.FloorId,
                Spots = new List<SpotDto>()
            };
        }

        public async Task<BayDto?> UpdateBayAsync(int id, BayDto bayDto)
        {
            var bay = await _context.Bays.FindAsync(id);
            if (bay == null) return null;

            bay.Name = bayDto.Name;
            bay.Location = bayDto.Location;
            bay.FloorId = bayDto.FloorId;

            await _context.SaveChangesAsync();

            return new BayDto
            {
                Id = bay.Id,
                Name = bay.Name,
                Location = bay.Location,
                FloorId = bay.FloorId,
                Spots = new List<SpotDto>()
            };
        }

        public async Task<bool> DeleteBayAsync(int id)
        {
            var bay = await _context.Bays.FindAsync(id);
            if (bay == null) return false;

            _context.Bays.Remove(bay);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}