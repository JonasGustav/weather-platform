using Microsoft.EntityFrameworkCore;
using WeatherPlatform.Common.Models;

namespace WeatherPlatform.Common.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Weather> WeatherReadings => Set<Weather>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Lat).HasColumnType("decimal(8,6)");
            entity.Property(e => e.Lon).HasColumnType("decimal(9,6)");
        });

        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Temp).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FeelsLike).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Uvi).HasColumnType("decimal(4,2)");
            entity.Property(e => e.WindSpeed).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Rain1h).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Snow1h).HasColumnType("decimal(6,2)");

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.WeatherReadings)
                  .HasForeignKey(e => e.LocationId);
        });
    }
}
