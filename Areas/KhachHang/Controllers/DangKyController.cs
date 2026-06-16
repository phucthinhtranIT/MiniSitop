using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;
using KhachHangModel = WebQLministop.Models.KhachHang;

namespace WebQLministop.Areas.KhachHang.Controllers
{
    [Area("KhachHang")]
    public class DangKyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DangKyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var daCoTaiKhoanIdentity = await _userManager.Users.AnyAsync(u =>
                u.LoaiTaiKhoan == "KhachHang" &&
                ((!string.IsNullOrWhiteSpace(email) && u.Email == email) ||
                 (!string.IsNullOrWhiteSpace(dienThoai) && u.PhoneNumber == dienThoai)));

            if (daTonTai || daCoTaiKhoanIdentity)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc số điện thoại đã được đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var khachHang = new KhachHangModel
            {
                HoTen = hoTen,
                DienThoai = dienThoai,
                Email = email,
                DiemThuong = 0,
                KichHoat = true
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
            var user = new ApplicationUser
            {
                UserName = $"khachhang-{khachHang.Id}",
                HoTen = khachHang.HoTen,
                Email = khachHang.Email,
                PhoneNumber = khachHang.DienThoai,
                LoaiTaiKhoan = "KhachHang",
                KhachHangId = khachHang.Id
            };

            var ketQua = await _userManager.CreateAsync(user, matKhau);
            if (!ketQua.Succeeded)
            {
                _context.KhachHangs.Remove(khachHang);
                await _context.SaveChangesAsync();

                foreach (var loi in ketQua.Errors)
                {
                    ModelState.AddModelError(string.Empty, loi.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(user, "KhachHang");
            khachHang.MatKhauHash = user.PasswordHash;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "DangNhap");
        }
    }
}
