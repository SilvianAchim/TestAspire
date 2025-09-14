using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;


var builder = DistributedApplication.CreateBuilder(args);

// Always manage Postgres via Aspire so it appears in the dashboard
// Note: resource names are case-insensitive; avoid 'Postgres' vs 'postgres' collisions.
var pg = builder.AddPostgres("pg").WithDataVolume();
var db = pg.AddDatabase("Postgres", databaseName: "mscoffee_dev");

// Existing TodoWeb
var web = builder.AddProject<Projects.TodoWeb>("web");

// Game1/2 reference the Aspire-managed database
var game1 = builder.AddProject<Projects.Game1>("game1")
    .WithReference(db)
    .WithEnvironment("ASPNETCORE_PATHBASE", "/game1");

var game2 = builder.AddProject<Projects.Game2>("game2")
    .WithReference(db)
    .WithEnvironment("ASPNETCORE_PATHBASE", "/game2");


builder.Build().Run();