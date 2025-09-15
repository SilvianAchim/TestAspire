# Rooms and Players - Common Logic Plan

This document proposes a reusable, persistence-first design for player identification and room linking, implemented in `MSCoffee.Common` and demoed in `src/Game2`.

## Goals
- Require each player to pick a nickname before any interaction.
- Create room: user gets a short unique join code.
- Join room by code: multiple players end up in the same room.
- Durable identity: player stays the same across refreshes / brief disconnects.
- Persist link between rooms and players in DB; be able to list player ids and names for a room.
- Service lives in `MSCoffee.Common` to be reused by any game.

## Non-goals (for this iteration)
- No real-time messaging / gameplay protocol.
- No authorization beyond lightweight player token.
- No UI polishing; only wiring needed for `Game2` demo endpoints.

## Data Model (Entity Framework)
- Player
  - Id (GUID)
  - Nickname (string, required, unique per room optional)
  - Token (GUID as string) — stable identifier stored in cookie/local storage; also indexed
  - CreatedAt (UTC)
  - LastSeenAt (UTC)
- Room
  - Id (GUID)
  - Code (string, 6-8 uppercase alphanum, unique, indexed)
  - CreatedAt (UTC)
  - ClosedAt (UTC, nullable)
- RoomMembership
  - Id (GUID)
  - RoomId (FK)
  - PlayerId (FK)
  - JoinedAt (UTC)
  - LeftAt (UTC, nullable)

Notes:
- Use separate membership table to support a player in multiple rooms over time and soft leaves.
- Add unique index (RoomId, PlayerId) to prevent duplicates.
- Optionally enforce unique nickname per room with filtered unique index on (RoomId, Nickname, LeftAt IS NULL).

## Player Identification Contract
- Client holds a `player_token` (GUID) set once:
  - If missing, backend issues a new token and returns it; client persists in cookie (httpOnly=false) or local storage.
- Backend uses `player_token` to look up or create a Player row on nickname registration.
- On each request, update `LastSeenAt`.

Error modes:
- Token present but no Player: treat as pre-registration state until nickname is set.
- Nickname change: allow updating nickname if not in any active room or allow per-room nickname via membership metadata (stretch).

## Workflows
1) Bootstrap
- Client GET to check session: if no token cookie, server generates token; response carries token and whether registered.

2) Set Nickname
- POST /players/register { token, nickname }
- Create Player if none by token; set nickname. Validate length (2-24), allowed chars, normalize spaces.

3) Create Room
- POST /rooms { token }
- Generates unique `Code`; create Room.
- Ensure Player exists; add RoomMembership if not already present.
- Return { roomId, code }.

4) Join Room
- POST /rooms/join { token, code }
- Lookup room by code; ensure Player exists and has nickname; upsert membership.
- Return { roomId, code }.

5) List Players in Room
- GET /rooms/{code}/players
- Returns [{ playerId, nickname, joinedAt, leftAt }].

6) Resilience
- Refresh: token persists; server matches Player by token; memberships are persisted.
- Brief disconnects: LastSeenAt is advisory; no impact to membership.

## Service Interfaces (in MSCoffee.Common)
- IPlayerSessionService
  - Task<(string token, bool existed)> EnsureTokenAsync(HttpContext ctx)
  - Task<Player> RegisterAsync(string token, string nickname)
  - Task<Player?> GetByTokenAsync(string token)
- IRoomService
  - Task<Room> CreateRoomAsync(Player player)
  - Task<Room> GetByCodeAsync(string code)
  - Task<RoomMembership> JoinRoomAsync(Room room, Player player)
  - Task<IReadOnlyList<(Guid playerId, string nickname)>> GetPlayersAsync(Room room)
- ICodeGenerator
  - string NewRoomCode(); // collision-resistant; retries until unique

Implementation details:
- Place entities in `MSCoffee.Common/Data/Entities`.
- Add DbSets: Players, Rooms, RoomMemberships in `CoffeeDbContext` and configure indexes.
- Migrations in `MSCoffee.Common/Migrations`.

## Code Generation for Room Codes
- 6 char base32 Crockford alphabet (20^6 ≈ 64M combos) or uppercase A-Z0-9 excluding ambiguous chars.
- Retry on collision (max 5 tries) before escalating.

## Validation
- Nickname: 2..24 chars, letters/digits/spaces/`-_`, trimmed, collapse multiple spaces.
- Code: uppercase A-Z0-9, length 6..8.

## Game2 Demo Wiring (minimal)
Add endpoints to `src/Game2`:
- GET /session -> ensures token; returns { token, registered }
- POST /players/register { nickname }
- POST /rooms { } -> create room for current player
- POST /rooms/join { code }
- GET /rooms/{code}/players

Each endpoint obtains/sets the token cookie and uses the common services.

Service registration in Game2:
- services.AddDbContext<CoffeeDbContext>(...)
- services.AddCommonRoomServices(); // extension in MSCoffee.Common to register IPlayerSessionService, IRoomService, ICodeGenerator

Cookie name: `player_token`; SameSite=Lax; expiration ~180 days.

## Edge Cases
- Duplicate nickname in same room: reject with 409 or append suffix (configurable).
- Room not found: 404 on join/list.
- Player without nickname attempts to join/create: 400 instructing to register.
- Token theft: low-risk demo; consider HMAC-signed token or httpOnly cookie in future.
- Cleanup: scheduled job to close inactive rooms (optional).

## Migration Steps
1) Add entities and DbSets.
2) Create migrations and update DB.
3) Add services and registration extension.
4) Wire demo endpoints in Game2.

## Success Criteria
- After registering nickname, a user can create a room and receive a code.
- Another user can join with the code.
- Listing players by code returns both players with stable ids and nicknames.
- Refreshing the page retains the same player id via token.
