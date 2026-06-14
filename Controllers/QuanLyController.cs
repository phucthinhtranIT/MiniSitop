using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class QuanLyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var sanPhams = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .OrderBy(p => p.Ten)
                .ToListAsync();

            var donHangs = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .Include(d => d.ChiTiet)
                .ThenInclude(c => c.SanPham)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            var khachHangs = await _context.KhachHangs
                .OrderBy(k => k.HoTen)
                .ToListAsync();

            var nhanViens = await _context.NhanViens
                .OrderBy(n => n.HoTen)
                .ToListAsync();

            var khuyenMais = await _context.KhuyenMais
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();

            ViewBag.SanPhams = sanPhams;
            ViewBag.DonHangs = donHangs;
            ViewBag.KhachHangs = khachHangs;
            ViewBag.NhanViens = nhanViens;
            ViewBag.KhuyenMais = khuyenMais;
            ViewBag.DanhMucs = await _context.DanhMucs.OrderBy(d => d.Ten).ToListAsync();
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.OrderBy(n => n.Ten).ToListAsync();

            ViewBag.DoanhThuHomNay = donHangs
                .Where(d => d.NgayDat >= today && d.NgayDat < tomorrow)
                .Sum(d => d.TongTien);
            ViewBag.SoHoaDonHomNay = donHangs.Count(d => d.NgayDat >= today && d.NgayDat < tomorrow);
            ViewBag.SanPhamSapHet = sanPhams.Count(p => p.TonKho <= p.MucCanNhapLai);
            ViewBag.NhanVienKichHoat = nhanViens.Count(n => n.KichHoat);
            ViewBag.KhachHangKichHoat = khachHangs.Count(k => k.KichHoat);
            ViewBag.KhuyenMaiDangChay = khuyenMais.Count(k => k.KichHoat && k.NgayBatDau <= DateTime.UtcNow && k.NgayKetThuc >= DateTime.UtcNow);
            ViewBag.KhuyenMaiSapHetHan = khuyenMais.Count(k => k.KichHoat && k.NgayKetThuc >= DateTime.UtcNow && k.NgayKetThuc <= DateTime.UtcNow.AddDays(7));
            ViewBag.KhuyenMaiDaKetThuc = khuyenMais.Count(k => k.NgayKetThuc < DateTime.UtcNow);
            ViewBag.TopSanPhams = donHangs
                .SelectMany(d => d.ChiTiet)
                .Where(c => c.SanPham != null)
                .GroupBy(c => c.SanPham!.Ten)
                .Select(g => new { Ten = g.Key, SoLuong = g.Sum(c => c.SoLuong) })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSanPham(SanPham sanPham)
        {
            if (await _context.SanPhams.AnyAsync(p => p.Ma == sanPham.Ma))
            {
                ModelState.AddModelError(nameof(SanPham.Ma), "Mã sản phẩm đã tồn tại.");
            }

            if (!await _context.DanhMucs.AnyAsync(d => d.Id == sanPham.DanhMucId))
            {
                ModelState.AddModelError(nameof(SanPham.DanhMucId), "Vui lòng chọn danh mục hợp lệ.");
            }

            if (!await _context.NhaCungCaps.AnyAsync(n => n.Id == sanPham.NhaCungCapId))
            {
                ModelState.AddModelError(nameof(SanPham.NhaCungCapId), "Vui lòng chọn nhà cung cấp hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                TempData["SanPhamMessage"] = "Không thể lưu sản phẩm. Vui lòng kiểm tra lại thông tin.";
                TempData["SanPhamMessageType"] = "danger";
                return RedirectToAction(nameof(Index), null, null, "products");
            }

            sanPham.KichHoat = true;
            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();

            TempData["SanPhamMessage"] = "Đã lưu sản phẩm mới.";
            TempData["SanPhamMessageType"] = "success";
            return RedirectToAction(nameof(Index), null, null, "products");
        }
    }
}
