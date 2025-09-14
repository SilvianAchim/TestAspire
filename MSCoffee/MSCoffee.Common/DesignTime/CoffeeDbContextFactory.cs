using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MSCoffee.Common.Data;
using MSCoffee.Common.Contants;

namespace MSCoffee.Common.DesignTime;

public class CoffeeDbContextFactory : IDesignTimeDbContextFactory<CoffeeDbContext>
{
    public CoffeeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoffeeDbContext>();
        // Prefer environment-provided connection string for design-time
        var cs =
            Environment.GetEnvironmentVariable(AppSettingsContants.Env_ConnectionStrings__Postgres)
            ?? Environment.GetEnvironmentVariable("ASPIRE_DEV_POSTGRES")
            ?? "Host=localhost;Port=5432;Database=mscoffee_dev;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(cs);
        return new CoffeeDbContext(optionsBuilder.Options);
    }
}
