namespace MSCoffee.Common.Data.Entities;

public class RoomMembership
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = default!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
}
