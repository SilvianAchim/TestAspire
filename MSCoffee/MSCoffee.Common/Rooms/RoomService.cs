using Microsoft.EntityFrameworkCore;
using MSCoffee.Common.Data;
using MSCoffee.Common.Data.Entities;

namespace MSCoffee.Common.Rooms;

public sealed class RoomService : IRoomService
{
    private readonly CoffeeDbContext _db;
    private readonly ICodeGenerator _codeGenerator;

    public RoomService(CoffeeDbContext db, ICodeGenerator codeGenerator)
    {
        _db = db;
        _codeGenerator = codeGenerator;
    }

    public async Task<Room> CreateRoomAsync(Player player, CancellationToken ct = default)
    {
        if (player.Id == Guid.Empty)
            throw new ArgumentException("Player must be persisted", nameof(player));

        var room = new Room { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };

        // Generate unique code with limited retries
        for (int i = 0; i < 5; i++)
        {
            var code = _codeGenerator.NewRoomCode();
            var exists = await _db.Rooms.AnyAsync(r => r.Code == code, ct);
            if (!exists)
            {
                room.Code = code;
                break;
            }
        }

        if (string.IsNullOrEmpty(room.Code))
            throw new InvalidOperationException("Failed to allocate room code");

        _db.Rooms.Add(room);
        _db.RoomMemberships.Add(new RoomMembership
        {
            Id = Guid.NewGuid(),
            Room = room,
            PlayerId = player.Id,
            JoinedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return room;
    }

    public async Task<Room> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        code = NormalizeCode(code);
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == code, ct)
            ?? throw new KeyNotFoundException("Room not found");
        return room;
    }

    public async Task<RoomMembership> JoinRoomAsync(Room room, Player player, CancellationToken ct = default)
    {
        var existing = await _db.RoomMemberships.FirstOrDefaultAsync(m => m.RoomId == room.Id && m.PlayerId == player.Id, ct);
        if (existing != null)
        {
            if (existing.LeftAt != null)
            {
                existing.LeftAt = null; // rejoin
                await _db.SaveChangesAsync(ct);
            }
            return existing;
        }

        var membership = new RoomMembership
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            PlayerId = player.Id,
            JoinedAt = DateTime.UtcNow
        };

        _db.RoomMemberships.Add(membership);
        await _db.SaveChangesAsync(ct);
        return membership;
    }

    public async Task<IReadOnlyList<(Guid playerId, string nickname)>> GetPlayersAsync(Room room, CancellationToken ct = default)
    {
        var q = from m in _db.RoomMemberships.AsNoTracking()
                join p in _db.Players.AsNoTracking() on m.PlayerId equals p.Id
                where m.RoomId == room.Id && m.LeftAt == null
                select new { p.Id, p.Nickname };

        var results = await q.ToListAsync(ct);
        return results.Select(r => (r.Id, r.Nickname)).ToList();
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));
        code = code.Trim().ToUpperInvariant();
        return code;
    }
}
