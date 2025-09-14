using TodoWeb.Models;


namespace TodoWeb.Services;


public sealed class InMemoryTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = new();
    private readonly object _lock = new();


    public List<TodoItem> All()
    {
        lock (_lock)
        {
            // Return a shallow copy to avoid accidental external mutations
            return _items.ToList();
        }
    }


    public void Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        lock (_lock)
        {
            var nextId = _items.Count == 0 ? 1 : (_items.Max(x => x.Id) + 1);
            _items.Add(new TodoItem(nextId, title.Trim(), false));
        }
    }


    public void Toggle(int id)
    {
        lock (_lock)
        {
            var i = _items.FindIndex(x => x.Id == id);
            if (i >= 0)
            {
                var t = _items[i];
                _items[i] = t with { IsDone = !t.IsDone };
            }
        }
    }


    public void Delete(int id)
    {
        lock (_lock)
        {
            _items.RemoveAll(x => x.Id == id);
        }
    }
}