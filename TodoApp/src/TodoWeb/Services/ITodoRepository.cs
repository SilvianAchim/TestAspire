using TodoWeb.Models;

namespace TodoWeb.Services;

public interface ITodoRepository
{
    List<TodoItem> All();
    void Add(string title);
    void Toggle(Guid id);
    void Delete(Guid id);
}