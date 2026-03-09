using Microsoft.EntityFrameworkCore;
using WeatherPlatform.Common.Data;
using WeatherPlatform.Common.Models;

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

    public async Task<IEnumerable<Weather>> GetLatestForLocationsAsync(IEnumerable<int> locationIds)
    {
        var latestIds = _context.WeatherReadings
            .Where(w => locationIds.Contains(w.LocationId))
            .GroupBy(w => w.LocationId)
            .Select(g => g.OrderByDescending(w => w.RecordedAt).Select(w => w.Id).First());

        return await _context.WeatherReadings
            .Include(w => w.Location)
            .Where(w => latestIds.Contains(w.Id))
            .ToListAsync();
    }

    public async Task<(IEnumerable<Weather> Items, int TotalCount)> GetHistoryAsync(
        IEnumerable<int> locationIds, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _context.WeatherReadings
            .Include(w => w.Location)
            .Where(w => locationIds.Contains(w.LocationId));

        if (from.HasValue) query = query.Where(w => w.RecordedAt >= from.Value);
        if (to.HasValue) query = query.Where(w => w.RecordedAt <= to.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(w => w.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Weather?> GetWarmestAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderByDescending(w => w.Temp).FirstOrDefaultAsync();

    public Task<Weather?> GetColdestAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderBy(w => w.Temp).FirstOrDefaultAsync();

    public Task<Weather?> GetCloudiestAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderByDescending(w => w.Clouds).FirstOrDefaultAsync();

    public Task<Weather?> GetHighestUviAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderByDescending(w => w.Uvi).FirstOrDefaultAsync();

    public Task<Weather?> GetFoggiestAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderBy(w => w.Visibility).FirstOrDefaultAsync();

    public Task<Weather?> GetWindiestAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).OrderByDescending(w => w.WindSpeed).FirstOrDefaultAsync();

    public Task<Weather?> GetMostRainAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).Where(w => w.Rain1h != null).OrderByDescending(w => w.Rain1h).FirstOrDefaultAsync();

    public Task<Weather?> GetMostSnowAsync(DateTime? from, DateTime? to) =>
        GetExtremeBaseQuery(from, to).Where(w => w.Snow1h != null).OrderByDescending(w => w.Snow1h).FirstOrDefaultAsync();

    private IQueryable<Weather> GetExtremeBaseQuery(DateTime? from, DateTime? to)
    {
        if (!from.HasValue && !to.HasValue)
        {
            var latestIds = _context.WeatherReadings
                .GroupBy(w => w.LocationId)
                .Select(g => g.OrderByDescending(w => w.RecordedAt).Select(w => w.Id).First());

            return _context.WeatherReadings
                .Include(w => w.Location)
                .Where(w => latestIds.Contains(w.Id));
        }

        var query = _context.WeatherReadings.Include(w => w.Location).AsQueryable();
        if (from.HasValue) query = query.Where(w => w.RecordedAt >= from.Value);
        if (to.HasValue) query = query.Where(w => w.RecordedAt <= to.Value);
        return query;
    }
}
