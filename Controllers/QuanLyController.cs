using Microsoft.AspNetCore.Mvc;

namespace WebQLministop.Controllers;

public class QuanLyController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "QuanLy", new { area = "QuanLy" });
    }
}
