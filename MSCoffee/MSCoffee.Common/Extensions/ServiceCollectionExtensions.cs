using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSCoffee.Common.Data;
using MSCoffee.Common.Contants;

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

    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
