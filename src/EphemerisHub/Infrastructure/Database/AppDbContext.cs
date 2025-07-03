using EphemerisHub.Infrastructure.Database.EntityConfiguration;
using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;

namespace EphemerisHub.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RinexGpsEphemeris> GpsEphemeris { get; set; }
    public DbSet<RinexGlonassEphemeris> GlonassEphemeris { get; set; }
    public DbSet<RinexGalileoEphemeris> GalileoEphemeris { get; set; }
    public DbSet<RinexBeidouEphemeris> BeidouEphemeris { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RinexGpsEphemerisConfiguration());
        modelBuilder.ApplyConfiguration(new RinexGlonassEphemerisConfiguration());
        modelBuilder.ApplyConfiguration(new RinexGalileoEphemerisConfiguration());
        modelBuilder.ApplyConfiguration(new RinexBeidouEphemerisConfiguration());
    }
}