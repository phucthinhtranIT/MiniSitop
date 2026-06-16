using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Areas.KhachHang.Controllers
{
    [Area("KhachHang")]
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
                var kw = keyword.Trim();
                query = query.Where(p => 
                    p.Ten.Contains(kw) || 
                    p.Ma.Contains(kw) || 
                    (p.DanhMuc != null && p.DanhMuc.Ten.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(danhMuc))
            {
                query = query.Where(p => p.DanhMuc != null && p.DanhMuc.Ten == danhMuc);
            }

            var sanPhams = await query.ToListAsync();

            sanPhams = sapXep switch
            {
                "gia-tang" => sanPhams.OrderBy(p => p.GiaBan).ToList(),
                "gia-giam" => sanPhams.OrderByDescending(p => p.GiaBan).ToList(),
                _ => sanPhams.OrderByDescending(p => p.Id).ToList()
            };

            ViewData["Keyword"] = keyword;
            ViewData["DanhMuc"] = danhMuc;
            ViewData["SapXep"] = sapXep;
            ViewBag.DanhMucs = await _context.DanhMucs
                .OrderBy(d => d.Ten)
                .ToListAsync();
            ViewBag.SanPhams = sanPhams.Take(40).ToList();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GoiYTimKiem(string? tuKhoa)
        {
            tuKhoa = tuKhoa?.Trim() ?? string.Empty;
            if (tuKhoa.Length == 0)
            {
                return Json(Array.Empty<object>());
            }

            var ketQua = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Where(p => p.KichHoat && (
                    p.Ten.Contains(tuKhoa) || 
                    p.Ma.Contains(tuKhoa) || 
                    (p.DanhMuc != null && p.DanhMuc.Ten.Contains(tuKhoa))))
                .OrderBy(p => p.Ten)
                .Take(8)
                .Select(p => new
                {
                    p.Id,
                    p.Ma,
                    p.Ten,
                    p.GiaBan,
                    DanhMuc = p.DanhMuc != null ? p.DanhMuc.Ten : null,
                    p.HinhAnh
                })
                .ToListAsync();

            return Json(ketQua);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
