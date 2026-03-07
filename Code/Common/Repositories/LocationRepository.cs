using Microsoft.EntityFrameworkCore;
using WeatherPlatform.Common.Data;
using WeatherPlatform.Common.Entities;

namespace WeatherPlatform.Common.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(int id) =>
        await _context.Locations.FindAsync(id);

    public async Task<IEnumerable<Location>> GetAllAsync() =>
        await _context.Locations.ToListAsync();

    public async Task AddAsync(Location entity)
    {
        await _context.Locations.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Location entity)
    {
        _context.Locations.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Locations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Location?> GetByCoordinatesAsync(decimal lat, decimal lon) =>
        await _context.Locations.FirstOrDefaultAsync(l => l.Lat == lat && l.Lon == lon);

    public async Task<IEnumerable<Location>> GetAllWithLatestWeatherAsync() =>
        await _context.Locations
            .Include(l => l.WeatherReadings.OrderByDescending(w => w.RecordedAt).Take(1))
            .ToListAsync();
}
