using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;


var builder = DistributedApplication.CreateBuilder(args);

// Always manage Postgres via Aspire so it appears in the dashboard
// Note: resource names are case-insensitive; avoid 'Postgres' vs 'postgres' collisions.
// Define a secret parameter for the Postgres password managed by Aspire
var pgPassword = builder.AddParameter("pg-password", secret: true);

var pg = builder.AddPostgres("pg")
    // Use the Aspire-managed parameter so connection strings and health checks match
    .WithPassword(pgPassword)
    .WithDataVolume();
var db = pg.AddDatabase("Postgres", databaseName: "mscoffee_dev");

// Existing TodoWeb references the database as well
var web = builder.AddProject<Projects.TodoWeb>("web")
    .WithReference(db);

// Game1/2 reference the Aspire-managed database
var game1 = builder.AddProject<Projects.Game1>("game1")
    .WithReference(db)
    .WithEnvironment("ASPNETCORE_PATHBASE", "/game1");

var game2 = builder.AddProject<Projects.Game2>("game2")
    .WithReference(db)
    .WithEnvironment("ASPNETCORE_PATHBASE", "/game2");


builder.Build().Run();