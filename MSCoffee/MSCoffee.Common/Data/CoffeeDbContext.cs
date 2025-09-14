using Microsoft.EntityFrameworkCore;

namespace MSCoffee.Common.Data;

public class CoffeeDbContext : DbContext
{
    public DbSet<SampleEntity> Samples => Set<SampleEntity>();

    public CoffeeDbContext(DbContextOptions<CoffeeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<SampleEntity>(b =>
        {
            b.ToTable("samples");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200);
        });
    }
}
