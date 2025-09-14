using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;


var builder = DistributedApplication.CreateBuilder(args);

// Check for an externally-provided Postgres connection string.
// If present, skip creating a container resource and pass it to the apps.
var externalCs = builder.Configuration["ConnectionStrings:Postgres"];

// Existing TodoWeb
var web = builder.AddProject<Projects.TodoWeb>("web");

if (!string.IsNullOrWhiteSpace(externalCs))
{
    // External Postgres mode
    var game1 = builder.AddProject<Projects.Game1>("game1")
        .WithEnvironment("ASPNETCORE_PATHBASE", "/game1")
        .WithEnvironment("ConnectionStrings__Postgres", externalCs);

    var game2 = builder.AddProject<Projects.Game2>("game2")
        .WithEnvironment("ASPNETCORE_PATHBASE", "/game2")
        .WithEnvironment("ConnectionStrings__Postgres", externalCs);
}
else
{
    // Containerized Postgres (requires a container runtime)
    var postgres = builder.AddPostgres("postgres").WithDataVolume();
    var db = postgres.AddDatabase("Postgres");

    var game1 = builder.AddProject<Projects.Game1>("game1")
        .WithReference(db)
        .WithEnvironment("ASPNETCORE_PATHBASE", "/game1");

    var game2 = builder.AddProject<Projects.Game2>("game2")
        .WithReference(db)
        .WithEnvironment("ASPNETCORE_PATHBASE", "/game2");
}


builder.Build().Run();