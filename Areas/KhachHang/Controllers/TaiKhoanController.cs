using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;
using KhachHangModel = WebQLministop.Models.KhachHang;

namespace WebQLministop.Areas.KhachHang.Controllers
{
    [Area("KhachHang")]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TaiKhoanController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var khachHang = await LayKhachHangDangNhap();
            if (khachHang == null)
            {
                return RedirectToAction("Index", "DangNhap", new { area = "KhachHang" });
            }

            return View(khachHang);
        }

        [HttpGet]
        public async Task<IActionResult> LichSuDonHang(int? donHangId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return RedirectToAction("Index", "DangNhap", new { area = "KhachHang" });
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
                return RedirectToAction("Index", "DangNhap", new { area = "KhachHang" });
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

            var user = await LayIdentityUser(khachHang);
            if (user != null)
            {
                user.HoTen = khachHang.HoTen;
                user.Email = khachHang.Email;
                user.PhoneNumber = khachHang.DienThoai;
                await _userManager.UpdateAsync(user);
            }

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
                return RedirectToAction("Index", "DangNhap", new { area = "KhachHang" });
            }

            var user = await LayIdentityUser(khachHang);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản đăng nhập tương ứng.");
                return View("Index", khachHang);
            }

            if (!await _userManager.HasPasswordAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản Google không sử dụng mật khẩu nội bộ.");
                return View("Index", khachHang);
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

            var ketQua = await _userManager.ChangePasswordAsync(user, matKhauHienTai, matKhauMoi);
            if (!ketQua.Succeeded)
            {
                foreach (var loi in ketQua.Errors)
                {
                    ModelState.AddModelError(string.Empty, loi.Description);
                }

                return View("Index", khachHang);
            }

            khachHang.MatKhauHash = user.PasswordHash;
            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);

            TempData["ThongBaoThanhCong"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<KhachHangModel?> LayKhachHangDangNhap()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId && k.KichHoat);
        }

        private async Task<ApplicationUser?> LayIdentityUser(KhachHangModel khachHang)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u =>
                u.LoaiTaiKhoan == "KhachHang" &&
                (u.KhachHangId == khachHang.Id ||
                 (!string.IsNullOrWhiteSpace(khachHang.Email) && u.Email == khachHang.Email) ||
                 (!string.IsNullOrWhiteSpace(khachHang.DienThoai) && u.PhoneNumber == khachHang.DienThoai)));
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

        private void LuuSessionKhachHang(KhachHangModel khachHang)
        {
            HttpContext.Session.SetString("KhachHangHoTen", khachHang.HoTen);
            HttpContext.Session.SetInt32("KhachHangDiemThuong", khachHang.DiemThuong);

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
