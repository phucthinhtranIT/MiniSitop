using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class DangKyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<KhachHang> _passwordHasher = new();

        public DangKyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string hoTen, string? dienThoai, string? email, string matKhau, string xacNhanMatKhau)
        {
            hoTen = hoTen?.Trim() ?? string.Empty;
            dienThoai = dienThoai?.Trim();
            email = email?.Trim();

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ModelState.AddModelError(nameof(hoTen), "Vui lòng nhập họ và tên.");
            }

            if (string.IsNullOrWhiteSpace(dienThoai) && string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(email), "Vui lòng nhập email hoặc số điện thoại.");
            }

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            {
                ModelState.AddModelError(nameof(email), "Email chưa đúng định dạng.");
            }

            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6)
            {
                ModelState.AddModelError(nameof(matKhau), "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (matKhau != xacNhanMatKhau)
            {
                ModelState.AddModelError(nameof(xacNhanMatKhau), "Mật khẩu xác nhận không khớp.");
            }

            var daTonTai = await _context.KhachHangs.AnyAsync(k =>
                (!string.IsNullOrWhiteSpace(email) && k.Email == email) ||
                (!string.IsNullOrWhiteSpace(dienThoai) && k.DienThoai == dienThoai));

            if (daTonTai)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc số điện thoại đã được đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var khachHang = new KhachHang
            {
                HoTen = hoTen,
                DienThoai = dienThoai,
                Email = email,
                DiemThuong = 0,
                KichHoat = true
            };
            khachHang.MatKhauHash = _passwordHasher.HashPassword(khachHang, matKhau);

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
            return RedirectToAction("Index", "DangNhap");
        }
    }
}
