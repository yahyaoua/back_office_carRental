namespace CarRental.Api.Controllers

{
    using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    // This action handles the default route (/)
    public IActionResult Index()
    {
        // View() tells the framework to look for Views/Home/Index.cshtml
        return View();
    }
} 

}