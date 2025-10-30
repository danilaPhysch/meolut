using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class GpsTleConfiguration : IEntityTypeConfiguration<GpsTle>
{
    public void Configure(EntityTypeBuilder<GpsTle> builder)
    {
        builder.HasKey(e => new { e.CsSatNum, e.Time });
    }
}
