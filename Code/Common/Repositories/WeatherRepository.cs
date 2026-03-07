using Microsoft.EntityFrameworkCore;
using WeatherPlatform.Common.Data;
using WeatherPlatform.Common.Entities;

namespace WeatherPlatform.Common.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _context;

    public WeatherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Weather?> GetByIdAsync(int id) =>
        await _context.WeatherReadings.FindAsync(id);

    public async Task<IEnumerable<Weather>> GetAllAsync() =>
        await _context.WeatherReadings.ToListAsync();

    public async Task AddAsync(Weather entity)
    {
        await _context.WeatherReadings.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Weather entity)
    {
        _context.WeatherReadings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.WeatherReadings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Weather?> GetLatestByLocationAsync(int locationId) =>
        await _context.WeatherReadings
            .Where(w => w.LocationId == locationId)
            .OrderByDescending(w => w.RecordedAt)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Weather>> GetByLocationAsync(int locationId) =>
        await _context.WeatherReadings
            .Where(w => w.LocationId == locationId)
            .OrderByDescending(w => w.RecordedAt)
            .ToListAsync();
}
