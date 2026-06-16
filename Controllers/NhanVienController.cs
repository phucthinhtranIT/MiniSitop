using Microsoft.AspNetCore.Mvc;

namespace WebQLministop.Controllers;

public class NhanVienController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "NhanVien", new { area = "NhanVien" });
    }
}
