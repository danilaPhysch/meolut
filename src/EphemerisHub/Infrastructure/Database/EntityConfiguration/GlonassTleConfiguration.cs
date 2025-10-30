using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class GlonassTleConfiguration : IEntityTypeConfiguration<GlonassTle>
{
    public void Configure(EntityTypeBuilder<GlonassTle> builder)
    {
        builder.HasKey(e => new { e.CsSatNum, e.Time });
    }
}
