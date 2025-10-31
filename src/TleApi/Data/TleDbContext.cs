using Microsoft.EntityFrameworkCore;
using TleApi.Models;

namespace TleApi.Data;

/// <summary>
/// Database context for TLE API
/// </summary>
public class TleDbContext : DbContext
{
    private readonly string _tableName;

    public TleDbContext(DbContextOptions<TleDbContext> options, string tableName = "tle") 
        : base(options)
    {
        _tableName = tableName;
    }

    public DbSet<TleEntity> TleData => Set<TleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TleEntity>(entity =>
        {
            entity.ToTable(_tableName);
            entity.HasNoKey(); // Read-only entity without primary key
            entity.Property(e => e.System).HasColumnName("system");
            entity.Property(e => e.Prn).HasColumnName("prn");
            entity.Property(e => e.Epoch).HasColumnName("epoch");
            entity.Property(e => e.Line1).HasColumnName("line1");
            entity.Property(e => e.Line2).HasColumnName("line2");
        });
    }
}
