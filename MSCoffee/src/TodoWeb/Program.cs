using MSCoffee.Common.Extensions;
using TodoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddCommonData(builder.Configuration);
builder.Services.AddScoped<ITodoRepository, EfTodoRepository>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Only TodosController is needed; root path handled there
app.MapControllers();

// Ensure DB is ready and schema applied; fail fast if unavailable
await app.Services.ApplyMigrationsAsync();

app.Run();