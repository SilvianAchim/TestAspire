using TodoWeb.Models;

namespace TodoWeb.Services;

public interface ITodoRepository
{
    List<TodoItem> All();
    void Add(string title);
    void Toggle(int id);
    void Delete(int id);
}