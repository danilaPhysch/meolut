using EphemerisHub.Infrastructure.Database.EntityConfiguration;
using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;

namespace EphemerisHub.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GpsTle> GpsTle { get; set; }
    public DbSet<GalileoTle> GalileoTle { get; set; }
    public DbSet<GlonassTle> GlonassTle { get; set; }
    public DbSet<BeidouTle> BeidouTle { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GpsTleConfiguration());
        modelBuilder.ApplyConfiguration(new GalileoTleConfiguration());
        modelBuilder.ApplyConfiguration(new GlonassTleConfiguration());
        modelBuilder.ApplyConfiguration(new BeidouTleConfiguration());
    }
}
