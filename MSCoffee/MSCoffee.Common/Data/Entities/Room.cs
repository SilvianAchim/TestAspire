using System.ComponentModel.DataAnnotations;

namespace MSCoffee.Common.Data.Entities;

public class Room
{
    public Guid Id { get; set; }

    [MaxLength(12)]
    public string Code { get; set; } = string.Empty; // Unique join code

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<RoomMembership> Memberships { get; set; } = new List<RoomMembership>();
}
