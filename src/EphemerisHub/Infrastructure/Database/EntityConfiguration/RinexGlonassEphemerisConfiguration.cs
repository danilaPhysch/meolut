using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class RinexGlonassEphemerisConfiguration : IEntityTypeConfiguration<RinexGlonassEphemeris>
{
    public void Configure(EntityTypeBuilder<RinexGlonassEphemeris> builder)
    {
        builder.HasKey(e => new { e.SatellitePrn, e.TimeOfClock });

        builder.Property(e => e.SatelliteSystem).IsRequired().HasMaxLength(1);
        builder.Property(e => e.PosX).HasPrecision(15, 3);
        builder.Property(e => e.PosY).HasPrecision(15, 3);
        builder.Property(e => e.PosZ).HasPrecision(15, 3);
        builder.Property(e => e.VelX).HasPrecision(15, 9);
        builder.Property(e => e.VelY).HasPrecision(15, 9);
        builder.Property(e => e.VelZ).HasPrecision(15, 9);
        builder.Property(e => e.AccX).HasPrecision(15, 12);
        builder.Property(e => e.AccY).HasPrecision(15, 12);
        builder.Property(e => e.AccZ).HasPrecision(15, 12);
    }
}