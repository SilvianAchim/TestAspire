using Microsoft.AspNetCore.Http;
using System.Linq;
using MSCoffee.Common.Extensions;
using MSCoffee.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();
builder.Services.AddCommonData(builder.Configuration);
builder.Services.AddCommonRoomServices();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

app.MapDefaultEndpoints();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

string EnsureTokenCookie(Microsoft.AspNetCore.Http.HttpContext ctx)
{
    const string CookieName = "player_token";
    if (!ctx.Request.Cookies.TryGetValue(CookieName, out var token) || string.IsNullOrWhiteSpace(token))
    {
        token = Guid.NewGuid().ToString("N");
        ctx.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(180)
        });
    }
    return token;
}

// Minimal API endpoints for demo
app.MapGet("/session", async (Microsoft.AspNetCore.Http.HttpContext ctx, MSCoffee.Common.Rooms.IPlayerSessionService sessions) =>
{
    var token = EnsureTokenCookie(ctx);
    var player = await sessions.GetByTokenAsync(token);
    return Results.Ok(new { token, registered = player != null });
});

app.MapPost("/players/register", async (Microsoft.AspNetCore.Http.HttpContext ctx, MSCoffee.Common.Rooms.IPlayerSessionService sessions, CancellationToken ct) =>
{
    var form = await ctx.Request.ReadFromJsonAsync<RegisterDto>(cancellationToken: ct);
    if (form is null || string.IsNullOrWhiteSpace(form.Nickname)) return Results.BadRequest(new { error = "Nickname required" });

    var token = EnsureTokenCookie(ctx);
    var player = await sessions.RegisterAsync(token, form.Nickname, ct);
    return Results.Ok(new { playerId = player.Id, nickname = player.Nickname });
});

app.MapPost("/rooms", async (Microsoft.AspNetCore.Http.HttpContext ctx, MSCoffee.Common.Rooms.IPlayerSessionService sessions, MSCoffee.Common.Rooms.IRoomService rooms, CancellationToken ct) =>
{
    var token = EnsureTokenCookie(ctx);
    var player = await sessions.GetByTokenAsync(token, ct);
    if (player is null || string.IsNullOrWhiteSpace(player.Nickname)) return Results.BadRequest(new { error = "Register nickname first" });
    var room = await rooms.CreateRoomAsync(player, ct);
    return Results.Ok(new { roomId = room.Id, code = room.Code });
});

app.MapPost("/rooms/join", async (Microsoft.AspNetCore.Http.HttpContext ctx, MSCoffee.Common.Rooms.IPlayerSessionService sessions, MSCoffee.Common.Rooms.IRoomService rooms, CancellationToken ct) =>
{
    var payload = await ctx.Request.ReadFromJsonAsync<JoinDto>(cancellationToken: ct);
    if (payload is null || string.IsNullOrWhiteSpace(payload.Code)) return Results.BadRequest(new { error = "Code required" });
    var token = EnsureTokenCookie(ctx);
    var player = await sessions.GetByTokenAsync(token, ct);
    if (player is null || string.IsNullOrWhiteSpace(player.Nickname)) return Results.BadRequest(new { error = "Register nickname first" });
    var room = await rooms.GetByCodeAsync(payload.Code, ct);
    await rooms.JoinRoomAsync(room, player, ct);
    return Results.Ok(new { roomId = room.Id, code = room.Code });
});

app.MapGet("/rooms/{code}/players", async (string code, MSCoffee.Common.Rooms.IRoomService rooms, CancellationToken ct) =>
{
    var room = await rooms.GetByCodeAsync(code, ct);
    var players = await rooms.GetPlayersAsync(room, ct);
    return Results.Ok(players.Select(p => new { playerId = p.playerId, nickname = p.nickname }));
});

if (app.Environment.IsDevelopment())
{
    await app.Services.ApplyMigrationsAsync();
}

app.Run();

public record RegisterDto(string Nickname);
public record JoinDto(string Code);
