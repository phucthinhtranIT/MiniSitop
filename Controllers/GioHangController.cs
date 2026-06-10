using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class GioHangController : Controller
    {
        private const string SessionKhuyenMaiId = "KhuyenMaiId";
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "DangNhap");
            }

            var gioHang = await LayGioHang(khachHangId.Value);
            ViewBag.KhuyenMai = await LayKhuyenMaiDangApDung();
            return View(gioHang);
        }

        [HttpPost]
        public async Task<IActionResult> Them(int sanPhamId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { thanhCong = false, canDangNhap = true, thongBao = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            var sanPham = await _context.SanPhams.FirstOrDefaultAsync(p => p.Id == sanPhamId && p.KichHoat);
            if (sanPham == null)
            {
                return Json(new { thanhCong = false, thongBao = "Sản phẩm không tồn tại hoặc đã ngừng bán." });
            }

            if (sanPham.TonKho <= 0)
            {
                return Json(new { thanhCong = false, thongBao = "Sản phẩm đã hết hàng." });
            }

            var gioHang = await LayHoacTaoGioHang(khachHangId.Value);
            var chiTiet = await _context.ChiTietGioHangs.FirstOrDefaultAsync(c => c.GioHangId == gioHang.Id && c.SanPhamId == sanPhamId);

            if (chiTiet == null)
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    GioHangId = gioHang.Id,
                    SanPhamId = sanPhamId,
                    SoLuong = 1
                });
            }
            else
            {
                if (chiTiet.SoLuong >= sanPham.TonKho)
                {
                    return Json(new { thanhCong = false, thongBao = "Số lượng trong giỏ đã bằng tồn kho hiện có." });
                }

                chiTiet.SoLuong += 1;
            }

            gioHang.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { thanhCong = true, thongBao = "Đã thêm sản phẩm vào giỏ hàng.", soLuong = await DemSoLuong(khachHangId.Value) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(int chiTietId, int soLuong)
        {
            var chiTiet = await LayChiTietCuaKhachHang(chiTietId);
            if (chiTiet == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (soLuong <= 0)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
            }
            else
            {
                chiTiet.SoLuong = Math.Min(soLuong, chiTiet.SanPham?.TonKho ?? soLuong);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int chiTietId)
        {
            var chiTiet = await LayChiTietCuaKhachHang(chiTietId);
            if (chiTiet != null)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCa()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var gioHang = await LayGioHang(khachHangId.Value);
                _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTiet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApDungKhuyenMai(string? maKhuyenMai)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập để áp dụng mã giảm giá.";
                return RedirectToAction("Index", "DangNhap");
            }

            maKhuyenMai = maKhuyenMai?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(maKhuyenMai))
            {
                TempData["ThongBaoGioHang"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction(nameof(Index));
            }

            var hienTai = DateTime.UtcNow;
            var khuyenMai = await _context.KhuyenMais.FirstOrDefaultAsync(k =>
                k.KichHoat &&
                k.Ma == maKhuyenMai &&
                k.NgayBatDau <= hienTai &&
                k.NgayKetThuc >= hienTai);

            if (khuyenMai == null)
            {
                HttpContext.Session.Remove(SessionKhuyenMaiId);
                TempData["ThongBaoGioHang"] = "Mã giảm giá không tồn tại, đã hết hạn hoặc chưa được kích hoạt.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetInt32(SessionKhuyenMaiId, khuyenMai.Id);
            TempData["ThongBaoGioHang"] = $"Đã áp dụng mã {khuyenMai.Ma}, giảm {khuyenMai.PhanTramGiam:N0}%.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BoKhuyenMai()
        {
            HttpContext.Session.Remove(SessionKhuyenMaiId);
            TempData["ThongBaoGioHang"] = "Đã bỏ mã giảm giá khỏi giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập để tạo đơn hàng.";
                return RedirectToAction("Index", "DangNhap");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTiet)
                .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(g => g.KhachHangId == khachHangId.Value);

            if (gioHang == null || !gioHang.ChiTiet.Any())
            {
                TempData["ThongBaoGioHang"] = "Giỏ hàng đang trống, chưa thể tạo đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            var nhanVien = await _context.NhanViens
                .Where(n => n.KichHoat)
                .OrderBy(n => n.Id)
                .FirstOrDefaultAsync();

            if (nhanVien == null)
            {
                TempData["ThongBaoGioHang"] = "Chưa có nhân viên hoạt động để tiếp nhận đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in gioHang.ChiTiet)
            {
                if (item.SanPham == null || !item.SanPham.KichHoat)
                {
                    TempData["ThongBaoGioHang"] = "Giỏ hàng có sản phẩm không còn bán. Vui lòng kiểm tra lại.";
                    return RedirectToAction(nameof(Index));
                }

                if (item.SoLuong > item.SanPham.TonKho)
                {
                    TempData["ThongBaoGioHang"] = $"Sản phẩm {item.SanPham.Ten} chỉ còn {item.SanPham.TonKho}. Vui lòng chỉnh số lượng.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var khuyenMai = await LayKhuyenMaiDangApDung();
            if (HttpContext.Session.GetInt32(SessionKhuyenMaiId) != null && khuyenMai == null)
            {
                HttpContext.Session.Remove(SessionKhuyenMaiId);
                TempData["ThongBaoGioHang"] = "Mã giảm giá đã hết hạn hoặc không còn hoạt động. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(Index));
            }

            var phanTramGiam = khuyenMai?.PhanTramGiam ?? 0m;
            var donHang = new DonHang
            {
                KhachHangId = khachHangId.Value,
                NhanVienId = nhanVien.Id,
                NgayDat = DateTime.UtcNow,
                TrangThai = "ChoXuLy",
                PhuongThucThanhToan = "TienMat",
                TongTien = 0m,
                ChiTiet = gioHang.ChiTiet.Select(item => new ChiTietDonHang
                {
                    SanPhamId = item.SanPhamId,
                    SoLuong = item.SoLuong,
                    DonGia = item.SanPham?.GiaBan ?? 0m,
                    TienGiam = Math.Round(item.SoLuong * (item.SanPham?.GiaBan ?? 0m) * phanTramGiam / 100m, 0)
                }).ToList()
            };

            donHang.TongTien = donHang.ChiTiet.Sum(item => item.SoLuong * item.DonGia - item.TienGiam);

            foreach (var item in gioHang.ChiTiet)
            {
                if (item.SanPham != null)
                {
                    item.SanPham.TonKho -= item.SoLuong;
                }
            }

            _context.DonHangs.Add(donHang);
            _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTiet);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(SessionKhuyenMaiId);

            TempData["ThongBaoGioHang"] = $"Đã tạo đơn hàng #{donHang.Id} thành công. Tổng tiền: {donHang.TongTien:N0}đ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SoLuong()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            var soLuong = khachHangId == null ? 0 : await DemSoLuong(khachHangId.Value);

            return Json(new { soLuong });
        }

        private async Task<GioHang> LayGioHang(int khachHangId)
        {
            return await _context.GioHangs
                .Include(g => g.ChiTiet)
                .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(g => g.KhachHangId == khachHangId)
                ?? new GioHang { KhachHangId = khachHangId };
        }

        private async Task<GioHang> LayHoacTaoGioHang(int khachHangId)
        {
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.KhachHangId == khachHangId);
            if (gioHang != null)
            {
                return gioHang;
            }

            gioHang = new GioHang { KhachHangId = khachHangId, NgayCapNhat = DateTime.UtcNow };
            _context.GioHangs.Add(gioHang);
            await _context.SaveChangesAsync();
            return gioHang;
        }

        private async Task<ChiTietGioHang?> LayChiTietCuaKhachHang(int chiTietId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null) return null;

            return await _context.ChiTietGioHangs
                .Include(c => c.GioHang)
                .Include(c => c.SanPham)
                .FirstOrDefaultAsync(c => c.Id == chiTietId && c.GioHang != null && c.GioHang.KhachHangId == khachHangId);
        }

        private async Task<int> DemSoLuong(int khachHangId)
        {
            return await _context.ChiTietGioHangs
                .Where(c => c.GioHang != null && c.GioHang.KhachHangId == khachHangId)
                .SumAsync(c => c.SoLuong);
        }

        private async Task<KhuyenMai?> LayKhuyenMaiDangApDung()
        {
            var khuyenMaiId = HttpContext.Session.GetInt32(SessionKhuyenMaiId);
            if (khuyenMaiId == null) return null;

            var hienTai = DateTime.UtcNow;
            return await _context.KhuyenMais.FirstOrDefaultAsync(k =>
                k.Id == khuyenMaiId.Value &&
                k.KichHoat &&
                k.NgayBatDau <= hienTai &&
                k.NgayKetThuc >= hienTai);
        }
    }
}
