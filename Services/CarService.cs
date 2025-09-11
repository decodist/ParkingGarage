using DemoAppDotNet.Classes;
using DemoAppDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoAppDotNet.Services;

public class CarService
{
    private readonly ApplicationDbContext _context;

    public CarService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Car>> GetAllCarsAsync()
    {
        return await _context.Cars.ToListAsync();
    }

    public async Task<Car?> GetCarByPlateAsync(string plate)
    {
        return await _context.Cars.FirstOrDefaultAsync(c => c.Plate == plate);
    }
    
    public async Task<Car?> GetCarByIdAsync(int id)
    {
        return await _context.Cars.FindAsync(id);
    }
    
    public async Task<List<Car>> SearchCarsByPlateAsync(string plate)
    {
        return await _context.Cars
            .Where(c => c.Plate.Contains(plate))
            .ToListAsync();
    }
    
    public async Task<List<Car>> GetUnparkedCarsAsync()
    {
        return await _context.Cars
            .Where(c => c.SpotId == null)
            .ToListAsync();
    }
    
    public async Task<bool> UpdateCarAsync(Car updatedCar)
    {
        var car = await _context.Cars.FindAsync(updatedCar.Id);
        if (car == null) return false;
    
        car.Plate = updatedCar.Plate;
        car.CheckIn = updatedCar.CheckIn;
        car.Size = updatedCar.Size;
        car.SpotId = updatedCar.SpotId;
        car.Meta = updatedCar.Meta;
    
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<Car> CreateCarAsync(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return car;
    }

    public async Task<bool> DeleteCarAsync(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return false;

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return true;
    }
}