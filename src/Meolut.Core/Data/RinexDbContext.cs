using Microsoft.EntityFrameworkCore;
using Meolut.Core.Models;

namespace Meolut.Core.Data;

public class RinexDbContext : DbContext
{
    public RinexDbContext(DbContextOptions<RinexDbContext> options) : base(options)
    {
    }

    public DbSet<GpsNavigationData> GpsNavigationData { get; set; }
    public DbSet<GlonassNavigationData> GlonassNavigationData { get; set; }
    public DbSet<GalileoNavigationData> GalileoNavigationData { get; set; }
    public DbSet<BeidouNavigationData> BeidouNavigationData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure GPS navigation data
        modelBuilder.Entity<GpsNavigationData>(entity =>
        {
            entity.HasIndex(e => new { e.SatellitePrn, e.EpochTime })
                .IsUnique()
                .HasDatabaseName("IX_GPS_Satellite_Epoch");
            
            entity.HasIndex(e => e.EpochTime)
                .HasDatabaseName("IX_GPS_Epoch");
            
            entity.Property(e => e.GnssSystem)
                .HasDefaultValue('G');
        });

        // Configure GLONASS navigation data
        modelBuilder.Entity<GlonassNavigationData>(entity =>
        {
            entity.HasIndex(e => new { e.SatellitePrn, e.EpochTime })
                .IsUnique()
                .HasDatabaseName("IX_GLONASS_Satellite_Epoch");
            
            entity.HasIndex(e => e.EpochTime)
                .HasDatabaseName("IX_GLONASS_Epoch");
            
            entity.Property(e => e.GnssSystem)
                .HasDefaultValue('R');
        });

        // Configure Galileo navigation data
        modelBuilder.Entity<GalileoNavigationData>(entity =>
        {
            entity.HasIndex(e => new { e.SatellitePrn, e.EpochTime })
                .IsUnique()
                .HasDatabaseName("IX_GALILEO_Satellite_Epoch");
            
            entity.HasIndex(e => e.EpochTime)
                .HasDatabaseName("IX_GALILEO_Epoch");
            
            entity.Property(e => e.GnssSystem)
                .HasDefaultValue('E');
        });

        // Configure BeiDou navigation data
        modelBuilder.Entity<BeidouNavigationData>(entity =>
        {
            entity.HasIndex(e => new { e.SatellitePrn, e.EpochTime })
                .IsUnique()
                .HasDatabaseName("IX_BEIDOU_Satellite_Epoch");
            
            entity.HasIndex(e => e.EpochTime)
                .HasDatabaseName("IX_BEIDOU_Epoch");
            
            entity.Property(e => e.GnssSystem)
                .HasDefaultValue('C');
        });
    }
}