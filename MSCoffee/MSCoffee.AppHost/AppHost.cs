using Aspire.Hosting;


var builder = DistributedApplication.CreateBuilder(args);


var web = builder.AddProject<Projects.TodoWeb>("web");


builder.Build().Run();