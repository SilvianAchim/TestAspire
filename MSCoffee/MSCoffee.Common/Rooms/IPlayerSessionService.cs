using MSCoffee.Common.Data.Entities;

namespace MSCoffee.Common.Rooms;

public interface IPlayerSessionService
{
    Task<Player> RegisterAsync(string token, string nickname, CancellationToken ct = default);
    Task<Player?> GetByTokenAsync(string token, CancellationToken ct = default);
}
