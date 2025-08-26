using TodoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Only TodosController is needed; root path handled there
app.MapControllers();

app.Run();