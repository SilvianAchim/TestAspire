using System.ComponentModel.DataAnnotations;

namespace MSCoffee.Common.Data.Entities;

public class Player
{
    public Guid Id { get; set; }

    [MaxLength(64)]
    public string Token { get; set; } = string.Empty; // Durable client token

    [MaxLength(64)]
    public string Nickname { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoomMembership> Memberships { get; set; } = new List<RoomMembership>();
}
