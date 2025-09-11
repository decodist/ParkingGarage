using DemoAppDotNet.Classes;
using DemoAppDotNet.DTOs;
using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoAppDotNet.Services
{
    public class SpotService
    {
        private readonly ApplicationDbContext _context;

        public SpotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SpotDto>> GetAllSpotsAsync()
        {
            var spots = await _context.Spots
                .Include(s => s.Floor)
                .ThenInclude(f => f.Building)
                .ToListAsync();

            return spots.Select(s => new SpotDto
            {
                Id = s.Id,
                Number = s.Number,
                Name = s.Name,
                FloorId = s.FloorId,
                BayId = s.BayId,
                BuildingId = s.Floor.BuildingId,
                Status = s.Status,
                Meta = s.Meta,
                MinuteRate = _context.Rates
                    .Where(r => r.SpotId == s.Id)
                    .Select(r => r.MinuteRate)
                    .FirstOrDefault(),
            });
        }
        
        public async Task<IEnumerable<SpotDto>> GetAvailableSpotsAsync()
        {
            var spots = await _context.Spots
                .Where(s => s.Status == "Available")
                .Include(s => s.Floor)
                .ThenInclude(f => f.Building)
                .ToListAsync();
    
            return spots.Select(s => new SpotDto
            {
                Id = s.Id,
                Number = s.Number,
                Name = s.Name,
                FloorId = s.FloorId,
                BayId = s.BayId,
                BuildingId = s.Floor.BuildingId,
                Status = s.Status,
                Meta = s.Meta,
                MinuteRate = _context.Rates
                    .Where(r => r.SpotId == s.Id)
                    .Select(r => r.MinuteRate)
                    .FirstOrDefault(),
            });
        }
        
        public async Task<SpotDto?> GetSpotByIdAsync(int id)
        {
            var spot = await _context.Spots
                .Include(s => s.Floor)
                .ThenInclude(f => f.Building)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (spot == null) return null;

            return new SpotDto
            {
                Id = spot.Id,
                Number = spot.Number,
                FloorId = spot.FloorId,
                BayId = spot.BayId,
                Status = spot.Status,
            };
        }

        public async Task<SpotDto> CreateSpotAsync(Spot spot)
        {
            if (spot == null)
            {
                throw new ArgumentNullException(nameof(spot), "Spot cannot be null.");
            }
        
            try
            {
                _context.Spots.Add(spot);
                await _context.SaveChangesAsync();
        
                return new SpotDto
                {
                    Id = spot.Id,
                    Number = spot.Number
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while creating the spot.", ex);
            }
        }

        public async Task<SpotDto?> UpdateSpotAsync(SpotDto updatedSpotDto)
        {
            var spot = await _context.Spots.FindAsync(updatedSpotDto.Id);
            if (spot == null) return null;
        
            spot.Number = updatedSpotDto.Number;
            spot.FloorId = updatedSpotDto.FloorId;
            spot.Status = updatedSpotDto.Status;
        
            await _context.SaveChangesAsync();
        
            return new SpotDto
            {
                Id = spot.Id,
                Number = spot.Number,
                Status = spot.Status
            };
        }

        public async Task<bool> DeleteSpotAsync(int id)
        {
            var spot = await _context.Spots.FindAsync(id);
            if (spot == null) return false;

            _context.Spots.Remove(spot);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}