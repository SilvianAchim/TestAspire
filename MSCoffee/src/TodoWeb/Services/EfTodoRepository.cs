using MSCoffee.Common.Data;
using Microsoft.EntityFrameworkCore;
using TodoWeb.Models;

namespace TodoWeb.Services;

public sealed class EfTodoRepository : ITodoRepository
{
    private readonly CoffeeDbContext _db;
    public EfTodoRepository(CoffeeDbContext db) => _db = db;

    public List<TodoItem> All()
        => _db.Samples.AsNoTracking().Select(x => new TodoItem(x.Id, x.Name, x.IsDone)).ToList();

    public void Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        _db.Samples.Add(new SampleEntity { Name = title.Trim(), IsDone = false });
        _db.SaveChanges();
    }

    public void Toggle(int id)
    {
        var e = _db.Samples.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        e.IsDone = !e.IsDone;
        _db.SaveChanges();
    }

    public void Delete(int id)
    {
        var e = _db.Samples.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        _db.Samples.Remove(e);
        _db.SaveChanges();
    }
}
