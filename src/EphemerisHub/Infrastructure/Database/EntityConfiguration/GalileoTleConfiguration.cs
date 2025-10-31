using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class GalileoTleConfiguration : IEntityTypeConfiguration<GalileoTle>
{
    public void Configure(EntityTypeBuilder<GalileoTle> builder)
    {
        builder.HasKey(e => new { e.CsSatNum, e.Time });
    }
}
