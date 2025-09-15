using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSCoffee.Common.Data;
using MSCoffee.Common.Contants;
using MSCoffee.Common.Rooms;

namespace MSCoffee.Common.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultConnectionName = "Postgres";

    public static IServiceCollection AddCommonData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? configuration[AppSettingsContants.ConnectionStrings_Postgres]
            ?? throw new InvalidOperationException("Missing Postgres connection string.");

        services.AddDbContext<CoffeeDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure();
            });
        });

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

        return services;
    }

    public static IServiceCollection AddCommonRoomServices(this IServiceCollection services)
    {
        services.AddScoped<ICodeGenerator, DefaultCodeGenerator>();
        services.AddScoped<IPlayerSessionService, PlayerSessionService>();
        services.AddScoped<IRoomService, RoomService>();
        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeDbContext>();

        const int maxAttempts = 20;
        var delay = TimeSpan.FromSeconds(2);
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                break;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(delay, cancellationToken);
                // optional: exponential backoff
                if (delay < TimeSpan.FromSeconds(10))
                    delay += TimeSpan.FromSeconds(1);
            }
        }
    }
}
