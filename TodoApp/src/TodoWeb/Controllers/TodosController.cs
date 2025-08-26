using Microsoft.AspNetCore.Mvc;
using TodoWeb.Models;
using TodoWeb.Services;


namespace TodoWeb.Controllers;


[IgnoreAntiforgeryToken] // Simpler for this HTMX demo; add proper antiforgery for production
public class TodosController : Controller
{
    private readonly ITodoRepository _repo;
    public TodosController(ITodoRepository repo) => _repo = repo;


    [HttpGet("/")]
    public IActionResult Index()
    => View("Index", new TodoViewModel(_repo.All()));


    [HttpPost("/todos")]
    public IActionResult Add([FromForm] string title)
    {
        _repo.Add(title);
        return PartialView("_TodoList", new TodoViewModel(_repo.All()));
    }


    [HttpPost("/todos/{id:guid}/toggle")]
    public IActionResult Toggle(Guid id)
    {
        _repo.Toggle(id);
        return PartialView("_TodoList", new TodoViewModel(_repo.All()));
    }


    [HttpDelete("/todos/{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _repo.Delete(id);
        return PartialView("_TodoList", new TodoViewModel(_repo.All()));
    }
}