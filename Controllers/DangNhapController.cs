using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<KhachHang> _passwordHasher = new();

        public DangNhapController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string taiKhoan, string matKhau)
        {
            taiKhoan = taiKhoan?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(taiKhoan) || string.IsNullOrWhiteSpace(matKhau))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tài khoản và mật khẩu.");
                return View();
            }

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k =>
                k.KichHoat &&
                (k.Email == taiKhoan || k.DienThoai == taiKhoan));

            if (khachHang == null || string.IsNullOrWhiteSpace(khachHang.MatKhauHash))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                return View();
            }

            var ketQua = _passwordHasher.VerifyHashedPassword(khachHang, khachHang.MatKhauHash, matKhau);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                return View();
            }

            LuuSessionKhachHang(khachHang);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangNhapGoogle()
        {
            var redirectUrl = Url.Action(nameof(DangNhapGoogleCallback), "DangNhap");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> DangNhapGoogleCallback()
        {
            var ketQua = await HttpContext.AuthenticateAsync("External");
            if (!ketQua.Succeeded || ketQua.Principal == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể đăng nhập bằng Google. Vui lòng thử lại.");
                return View(nameof(Index));
            }

            var email = ketQua.Principal.FindFirst(ClaimTypes.Email)?.Value?.Trim();
            var hoTen = ketQua.Principal.FindFirst(ClaimTypes.Name)?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync("External");
                ModelState.AddModelError(string.Empty, "Tài khoản Google này không cung cấp email.");
                return View(nameof(Index));
            }

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.KichHoat && k.Email == email);
            if (khachHang == null)
            {
                khachHang = new KhachHang
                {
                    HoTen = string.IsNullOrWhiteSpace(hoTen) ? email : hoTen,
                    Email = email,
                    KichHoat = true
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();
            }

            LuuSessionKhachHang(khachHang);
            await HttpContext.SignOutAsync("External");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(string email)
        {
            email = email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(email), "Vui lòng nhập email đã đăng ký.");
                return View();
            }

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.KichHoat && k.Email == email);
            if (khachHang == null)
            {
                TempData["ThongBao"] = "Nếu email đã được đăng ký, hệ thống sẽ gửi đường link đặt lại mật khẩu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            khachHang.MaDatLaiMatKhau = Guid.NewGuid().ToString("N");
            khachHang.HanDatLaiMatKhau = DateTime.UtcNow.AddMinutes(30);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action(nameof(DatLaiMatKhau), "DangNhap", new { token = khachHang.MaDatLaiMatKhau }, Request.Scheme) ?? string.Empty;
            var daGuiEmail = await GuiEmailDatLaiMatKhau(khachHang.Email!, resetLink);

            TempData["ThongBao"] = daGuiEmail
                ? "Đường link đặt lại mật khẩu đã được gửi tới email của bạn."
                : $"SMTP chưa bật nên chưa gửi email thật. Link test: {resetLink}";

            return RedirectToAction(nameof(QuenMatKhau));
        }

        [HttpGet]
        public async Task<IActionResult> DatLaiMatKhau(string token)
        {
            var khachHang = await TimKhachHangTheoToken(token);
            if (khachHang == null)
            {
                TempData["ThongBao"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(string token, string matKhau, string xacNhanMatKhau)
        {
            var khachHang = await TimKhachHangTheoToken(token);
            if (khachHang == null)
            {
                TempData["ThongBao"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6)
            {
                ModelState.AddModelError(nameof(matKhau), "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            if (matKhau != xacNhanMatKhau)
            {
                ModelState.AddModelError(nameof(xacNhanMatKhau), "Mật khẩu xác nhận không khớp.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Token = token;
                return View();
            }

            khachHang.MatKhauHash = _passwordHasher.HashPassword(khachHang, matKhau);
            khachHang.MaDatLaiMatKhau = null;
            khachHang.HanDatLaiMatKhau = null;
            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Remove("KhachHangId");
            HttpContext.Session.Remove("KhachHangHoTen");
            HttpContext.Session.Remove("KhachHangAnhDaiDien");
            HttpContext.Session.Remove("KhachHangDiemThuong");

            return RedirectToAction("Index", "Home");
        }

        private void LuuSessionKhachHang(KhachHang khachHang)
        {
            HttpContext.Session.SetInt32("KhachHangId", khachHang.Id);
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

        private async Task<KhachHang?> TimKhachHangTheoToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k =>
                k.KichHoat &&
                k.MaDatLaiMatKhau == token &&
                k.HanDatLaiMatKhau != null &&
                k.HanDatLaiMatKhau > DateTime.UtcNow);
        }

        private async Task<bool> GuiEmailDatLaiMatKhau(string email, string resetLink)
        {
            if (!_configuration.GetValue<bool>("Smtp:Enabled"))
            {
                return false;
            }

            var host = _configuration["Smtp:Host"];
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"] ?? "MiniStop";
            var port = _configuration.GetValue<int>("Smtp:Port");
            var enableSsl = _configuration.GetValue<bool>("Smtp:EnableSsl");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            {
                return false;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Đặt lại mật khẩu MiniStop",
                Body = $"Bạn bấm vào link này để đặt lại mật khẩu trong 30 phút: {resetLink}",
                IsBodyHtml = false
            };
            message.To.Add(email);

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                smtp.Credentials = new NetworkCredential(username, password);
            }

            await smtp.SendMailAsync(message);
            return true;
        }
    }
}
