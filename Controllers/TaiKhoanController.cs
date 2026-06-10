using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly PasswordHasher<KhachHang> _passwordHasher = new();

        public TaiKhoanController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var khachHang = await LayKhachHangDangNhap();
            if (khachHang == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            return View(khachHang);
        }

        [HttpGet]
        public async Task<IActionResult> LichSuDonHang(int? donHangId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            var donHangs = await _context.DonHangs
                .Include(d => d.ChiTiet)
                .ThenInclude(c => c.SanPham)
                .Where(d => d.KhachHangId == khachHangId.Value)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            ViewBag.DonHangChiTiet = donHangId == null ? null : donHangs.FirstOrDefault(d => d.Id == donHangId.Value);
            return View(donHangs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTin(string hoTen, string? email, string? dienThoai, IFormFile? anhDaiDien)
        {
            var khachHang = await LayKhachHangDangNhap();
            if (khachHang == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            hoTen = hoTen?.Trim() ?? string.Empty;
            email = email?.Trim();
            dienThoai = dienThoai?.Trim();

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ModelState.AddModelError(nameof(hoTen), "Vui lòng nhập họ và tên.");
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(dienThoai))
            {
                ModelState.AddModelError(nameof(email), "Vui lòng nhập email hoặc số điện thoại.");
            }

            var daTonTai = await _context.KhachHangs.AnyAsync(k =>
                k.Id != khachHang.Id &&
                ((!string.IsNullOrWhiteSpace(email) && k.Email == email) ||
                 (!string.IsNullOrWhiteSpace(dienThoai) && k.DienThoai == dienThoai)));

            if (daTonTai)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc số điện thoại đã được tài khoản khác sử dụng.");
            }

            if (anhDaiDien != null && anhDaiDien.Length > 0)
            {
                var duoiFile = Path.GetExtension(anhDaiDien.FileName).ToLowerInvariant();
                var duoiHopLe = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!duoiHopLe.Contains(duoiFile))
                {
                    ModelState.AddModelError(nameof(anhDaiDien), "Ảnh đại diện chỉ nhận JPG, PNG hoặc WEBP.");
                }

                if (anhDaiDien.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(anhDaiDien), "Ảnh đại diện không được quá 2MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", khachHang);
            }

            khachHang.HoTen = hoTen;
            khachHang.Email = email;
            khachHang.DienThoai = dienThoai;

            if (anhDaiDien != null && anhDaiDien.Length > 0)
            {
                khachHang.AnhDaiDien = await LuuAnhDaiDien(khachHang.Id, anhDaiDien);
            }

            await _context.SaveChangesAsync();
            LuuSessionKhachHang(khachHang);

            TempData["ThongBaoThanhCong"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string matKhauHienTai, string matKhauMoi, string xacNhanMatKhauMoi)
        {
            var khachHang = await LayKhachHangDangNhap();
            if (khachHang == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            if (string.IsNullOrWhiteSpace(khachHang.MatKhauHash) ||
                _passwordHasher.VerifyHashedPassword(khachHang, khachHang.MatKhauHash, matKhauHienTai) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
            }

            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
            {
                ModelState.AddModelError(nameof(matKhauMoi), "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            if (matKhauMoi != xacNhanMatKhauMoi)
            {
                ModelState.AddModelError(nameof(xacNhanMatKhauMoi), "Mật khẩu xác nhận không khớp.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", khachHang);
            }

            khachHang.MatKhauHash = _passwordHasher.HashPassword(khachHang, matKhauMoi);
            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<KhachHang?> LayKhachHangDangNhap()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId && k.KichHoat);
        }

        private async Task<string> LuuAnhDaiDien(int khachHangId, IFormFile file)
        {
            var thuMuc = Path.Combine(_environment.WebRootPath, "uploads", "khachhang");
            Directory.CreateDirectory(thuMuc);

            var tenFile = $"khach-{khachHangId}-{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var duongDanVatLy = Path.Combine(thuMuc, tenFile);

            await using var stream = System.IO.File.Create(duongDanVatLy);
            await file.CopyToAsync(stream);

            return $"/uploads/khachhang/{tenFile}";
        }

        private void LuuSessionKhachHang(KhachHang khachHang)
        {
            HttpContext.Session.SetString("KhachHangHoTen", khachHang.HoTen);

            if (string.IsNullOrWhiteSpace(khachHang.AnhDaiDien))
            {
                HttpContext.Session.Remove("KhachHangAnhDaiDien");
            }
            else
            {
                HttpContext.Session.SetString("KhachHangAnhDaiDien", khachHang.AnhDaiDien);
            }
        }
    }
}
