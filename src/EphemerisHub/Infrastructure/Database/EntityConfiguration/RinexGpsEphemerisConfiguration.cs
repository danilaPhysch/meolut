using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class RinexGpsEphemerisConfiguration : IEntityTypeConfiguration<RinexGpsEphemeris>
{
    public void Configure(EntityTypeBuilder<RinexGpsEphemeris> builder)
    {
        builder.HasKey(e => new { e.SatellitePrn, e.TimeOfClock });

        builder.Property(e => e.SatelliteSystem).IsRequired().HasMaxLength(1);
        builder.Property(e => e.SatellitePrn).IsRequired();
        builder.Property(e => e.TimeOfClock).IsRequired();
        builder.Property(e => e.ClockBias).HasPrecision(15, 12);
        builder.Property(e => e.ClockDrift).HasPrecision(15, 12);
        builder.Property(e => e.ClockDriftRate).HasPrecision(15, 12);

        // GPS специфичные поля
        builder.Property(e => e.Iode).HasPrecision(15, 12);
        builder.Property(e => e.Crs).HasPrecision(15, 12);
        builder.Property(e => e.DeltaN).HasPrecision(15, 12);
        builder.Property(e => e.M0).HasPrecision(15, 12);
        builder.Property(e => e.Cuc).HasPrecision(15, 12);
        builder.Property(e => e.Eccentricity).HasPrecision(15, 12);
        builder.Property(e => e.Cus).HasPrecision(15, 12);
        builder.Property(e => e.SqrtA).HasPrecision(15, 12);
        builder.Property(e => e.Toe).HasPrecision(15, 12);
        builder.Property(e => e.Cic).HasPrecision(15, 12);
        builder.Property(e => e.Omega0).HasPrecision(15, 12);
        builder.Property(e => e.Cis).HasPrecision(15, 12);
        builder.Property(e => e.I0).HasPrecision(15, 12);
        builder.Property(e => e.Crc).HasPrecision(15, 12);
        builder.Property(e => e.Omega).HasPrecision(15, 12);
        builder.Property(e => e.OmegaDot).HasPrecision(15, 12);
        builder.Property(e => e.Idot).HasPrecision(15, 12);
        builder.Property(e => e.Iodc).HasPrecision(15, 12);
        builder.Property(e => e.SvAccuracy).HasPrecision(15, 12);
        builder.Property(e => e.SvHealth).HasPrecision(15, 12);
        builder.Property(e => e.Tgd).HasPrecision(15, 12);
        builder.Property(e => e.TransmissionTime).HasPrecision(15, 12);
        builder.Property(e => e.FitInterval).HasPrecision(15, 12);
    }
}