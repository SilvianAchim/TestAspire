using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
