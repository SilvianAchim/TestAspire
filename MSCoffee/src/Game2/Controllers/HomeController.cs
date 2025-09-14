using Microsoft.AspNetCore.Mvc;

namespace Game2.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
