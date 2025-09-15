using Microsoft.EntityFrameworkCore;
using MSCoffee.Common.Data.Entities;

namespace MSCoffee.Common.Data;

public class CoffeeDbContext : DbContext
{
    public DbSet<SampleEntity> Samples => Set<SampleEntity>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMembership> RoomMemberships => Set<RoomMembership>();

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
            b.Property(x => x.IsDone);
        });

        modelBuilder.Entity<Player>(b =>
        {
            b.ToTable("players");
            b.HasKey(x => x.Id);
            b.Property(x => x.Token).IsRequired().HasMaxLength(64);
            b.Property(x => x.Nickname).IsRequired().HasMaxLength(64);
            b.Property(x => x.CreatedAt);
            b.Property(x => x.LastSeenAt);
            b.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<Room>(b =>
        {
            b.ToTable("rooms");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(12);
            b.Property(x => x.CreatedAt);
            b.Property(x => x.ClosedAt);
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RoomMembership>(b =>
        {
            b.ToTable("room_memberships");
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Room)
                .WithMany(r => r.Memberships)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Player)
                .WithMany(p => p.Memberships)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.JoinedAt);
            b.Property(x => x.LeftAt);

            b.HasIndex(x => new { x.RoomId, x.PlayerId }).IsUnique();
        });
    }
}
