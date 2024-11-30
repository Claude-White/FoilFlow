using Microsoft.AspNetCore.Mvc;

namespace CWhiteH60Services.Controllers;

public class OrderItemController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
}