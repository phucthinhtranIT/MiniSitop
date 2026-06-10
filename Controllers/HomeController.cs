using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Where(p => p.KichHoat)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.DanhMucs = await _context.DanhMucs
                .OrderBy(d => d.Ten)
                .ToListAsync();
            ViewBag.SanPhamsMoi = sanPhams.Take(5).ToList();
            ViewBag.SanPhamsBanChay = sanPhams
                .OrderByDescending(p => p.TonKho)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TimKiem(string? keyword, string? danhMuc, string? sapXep)
        {
            var query = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Where(p => p.KichHoat);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Ten.Contains(keyword) ||
                    p.Ma.Contains(keyword) ||
                    (p.DanhMuc != null && p.DanhMuc.Ten.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(danhMuc))
            {
                query = query.Where(p => p.DanhMuc != null && p.DanhMuc.Ten == danhMuc);
            }

            query = sapXep switch
            {
                "gia-tang" => query.OrderBy(p => p.GiaBan),
                "gia-giam" => query.OrderByDescending(p => p.GiaBan),
                _ => query.OrderByDescending(p => p.Id)
            };

            ViewData["Keyword"] = keyword;
            ViewData["DanhMuc"] = danhMuc;
            ViewData["SapXep"] = sapXep;
            ViewBag.DanhMucs = await _context.DanhMucs
                .OrderBy(d => d.Ten)
                .ToListAsync();
            ViewBag.SanPhams = await query.Take(40).ToListAsync();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
