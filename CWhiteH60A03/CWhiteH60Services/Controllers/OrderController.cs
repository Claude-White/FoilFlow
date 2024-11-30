using Microsoft.AspNetCore.Mvc;

namespace CWhiteH60Services.Controllers;

public class OrderController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
}