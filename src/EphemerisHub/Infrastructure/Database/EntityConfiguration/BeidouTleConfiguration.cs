using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EphemerisHub.Infrastructure.Database.EntityConfiguration;

public class BeidouTleConfiguration : IEntityTypeConfiguration<BeidouTle>
{
    public void Configure(EntityTypeBuilder<BeidouTle> builder)
    {
        builder.HasKey(e => new { e.CsSatNum, e.Time });
    }
}
