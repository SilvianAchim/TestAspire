using Microsoft.EntityFrameworkCore;
using MSCoffee.Common.Data;
using MSCoffee.Common.Data.Entities;

namespace MSCoffee.Common.Rooms;

public sealed class PlayerSessionService : IPlayerSessionService
{
    private readonly CoffeeDbContext _db;

    public PlayerSessionService(CoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<Player> RegisterAsync(string token, string nickname, CancellationToken ct = default)
    {
        nickname = ValidateNickname(nickname);

        var player = await _db.Players.FirstOrDefaultAsync(p => p.Token == token, ct);
        if (player is null)
        {
            player = new Player
            {
                Id = Guid.NewGuid(),
                Token = token,
                Nickname = nickname,
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };
            _db.Players.Add(player);
        }
        else
        {
            player.Nickname = nickname;
            player.LastSeenAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return player;
    }

    public async Task<Player?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        var player = await _db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Token == token, ct);
        if (player != null)
        {
            // Update last seen fire-and-forget
            var tracked = await _db.Players.FirstOrDefaultAsync(p => p.Id == player.Id, ct);
            if (tracked != null)
            {
                tracked.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }
        return player;
    }

    // Note: Cookie issuance is handled at the app layer; the common service is persistence-only.

    private static string ValidateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Nickname is required", nameof(nickname));

        nickname = nickname.Trim();
        if (nickname.Length < 2 || nickname.Length > 24)
            throw new ArgumentException("Nickname must be 2-24 characters");

        // Allow letters, digits, space, dash, underscore
        foreach (var ch in nickname)
        {
            if (!(char.IsLetterOrDigit(ch) || ch == ' ' || ch == '-' || ch == '_'))
                throw new ArgumentException("Nickname contains invalid characters");
        }

        return System.Text.RegularExpressions.Regex.Replace(nickname, @"\s+", " ");
    }
}
