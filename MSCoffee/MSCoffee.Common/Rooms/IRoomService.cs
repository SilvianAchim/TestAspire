using MSCoffee.Common.Data.Entities;

namespace MSCoffee.Common.Rooms;

public interface IRoomService
{
    Task<Room> CreateRoomAsync(Player player, CancellationToken ct = default);
    Task<Room> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<RoomMembership> JoinRoomAsync(Room room, Player player, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid playerId, string nickname)>> GetPlayersAsync(Room room, CancellationToken ct = default);
}
