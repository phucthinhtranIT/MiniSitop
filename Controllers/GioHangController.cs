using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

namespace WebQLministop.Controllers
{
    public class GioHangController : Controller
    {
        private const string SessionKhuyenMaiId = "KhuyenMaiId";
        private const string SessionDiemThuongSuDung = "DiemThuongSuDung";
        private const decimal GiaTriMoiDiem = 1m;
        private const decimal SoTienMoiDiemThuong = 100m;
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
            var khuyenMai = await LayKhuyenMaiDangApDung();
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId.Value && k.KichHoat);
            var tongSauKhuyenMai = TinhTongSauKhuyenMai(gioHang, khuyenMai);

            ViewBag.KhuyenMai = khuyenMai;
            ViewBag.DiemThuong = khachHang?.DiemThuong ?? 0;
            ViewBag.DiemThuongSuDung = LayDiemThuongSuDungHopLe(khachHang?.DiemThuong ?? 0, tongSauKhuyenMai);
            ViewBag.GiaTriMoiDiem = GiaTriMoiDiem;
            return View(gioHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> ApDungDiemThuong(int diemSuDung)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập để sử dụng điểm tích lũy.";
                return RedirectToAction("Index", "DangNhap");
            }

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId.Value && k.KichHoat);
            if (khachHang == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập lại để sử dụng điểm tích lũy.";
                return RedirectToAction("Index", "DangNhap");
            }

            var gioHang = await LayGioHang(khachHangId.Value);
            if (!gioHang.ChiTiet.Any())
            {
                HttpContext.Session.Remove(SessionDiemThuongSuDung);
                TempData["ThongBaoGioHang"] = "Giỏ hàng đang trống, chưa thể dùng điểm tích lũy.";
                return RedirectToAction(nameof(Index));
            }

            var tongSauKhuyenMai = TinhTongSauKhuyenMai(gioHang, await LayKhuyenMaiDangApDung());
            var diemToiDa = Math.Min(khachHang.DiemThuong, (int)Math.Floor(tongSauKhuyenMai / GiaTriMoiDiem));
            if (diemToiDa <= 0)
            {
                HttpContext.Session.Remove(SessionDiemThuongSuDung);
                TempData["ThongBaoGioHang"] = "Bạn chưa có điểm tích lũy khả dụng cho đơn hàng này.";
                return RedirectToAction(nameof(Index));
            }

            var diemHopLe = Math.Clamp(diemSuDung, 1, diemToiDa);
            HttpContext.Session.SetInt32(SessionDiemThuongSuDung, diemHopLe);
            TempData["ThongBaoGioHang"] = $"Đã áp dụng {diemHopLe:N0} điểm tích lũy, giảm {(diemHopLe * GiaTriMoiDiem):N0}đ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BoDiemThuong()
        {
            HttpContext.Session.Remove(SessionDiemThuongSuDung);
            TempData["ThongBaoGioHang"] = "Đã bỏ điểm tích lũy khỏi giỏ hàng.";
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

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId.Value && k.KichHoat);
            if (khachHang == null)
            {
                TempData["ThongBaoDangNhap"] = "Vui lòng đăng nhập lại để tạo đơn hàng.";
                return RedirectToAction("Index", "DangNhap");
            }

            var phanTramGiam = khuyenMai?.PhanTramGiam ?? 0m;
            var tongTienGoc = gioHang.ChiTiet.Sum(item => item.SoLuong * (item.SanPham?.GiaBan ?? 0m));
            var tongSauKhuyenMai = TinhTongSauKhuyenMai(gioHang, khuyenMai);
            var diemSuDung = LayDiemThuongSuDungHopLe(khachHang.DiemThuong, tongSauKhuyenMai);
            var tienGiamTuDiem = diemSuDung * GiaTriMoiDiem;
            var donHang = new DonHang
            {
                KhachHangId = khachHangId.Value,
                NhanVienId = nhanVien.Id,
                NgayDat = DateTime.UtcNow,
                TrangThai = "ThanhCong",
                PhuongThucThanhToan = "TienMat",
                TongTien = 0m,
                DiemThuongSuDung = diemSuDung,
                TienGiamDiem = tienGiamTuDiem,
                MaKhuyenMaiDaDung = khuyenMai?.Ma,
                ChiTiet = gioHang.ChiTiet.Select(item => new ChiTietDonHang
                {
                    SanPhamId = item.SanPhamId,
                    SoLuong = item.SoLuong,
                    DonGia = item.SanPham?.GiaBan ?? 0m,
                    TienGiam = Math.Round(item.SoLuong * (item.SanPham?.GiaBan ?? 0m) * phanTramGiam / 100m, 0)
                }).ToList()
            };

            ApDungTienGiamTuDiem(donHang.ChiTiet, tienGiamTuDiem);
            donHang.TongTien = donHang.ChiTiet.Sum(item => item.SoLuong * item.DonGia - item.TienGiam);
            donHang.TienGiamKhuyenMai = donHang.ChiTiet.Sum(item => Math.Round(item.SoLuong * item.DonGia * phanTramGiam / 100m, 0));
            var diemCong = (int)Math.Floor(tongTienGoc / SoTienMoiDiemThuong);
            donHang.DiemThuongCong = diemCong;
            khachHang.DiemThuong -= diemSuDung;
            khachHang.DiemThuong += diemCong;

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
            HttpContext.Session.Remove(SessionDiemThuongSuDung);
            HttpContext.Session.SetInt32("KhachHangDiemThuong", khachHang.DiemThuong);

            TempData["ThongBaoGioHang"] = $"Đã tạo đơn hàng #{donHang.Id} thành công. Tổng tiền: {donHang.TongTien:N0}đ. Bạn được cộng {diemCong:N0} điểm tích lũy.";
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

        private int LayDiemThuongSuDungHopLe(int diemThuong, decimal tongSauKhuyenMai)
        {
            var diemTrongSession = HttpContext.Session.GetInt32(SessionDiemThuongSuDung) ?? 0;
            var diemToiDaTheoDon = Math.Max(0, (int)Math.Floor(tongSauKhuyenMai / GiaTriMoiDiem));
            var diemToiDa = Math.Min(diemThuong, diemToiDaTheoDon);
            var diemHopLe = Math.Clamp(diemTrongSession, 0, diemToiDa);

            if (diemHopLe != diemTrongSession)
            {
                if (diemHopLe <= 0)
                {
                    HttpContext.Session.Remove(SessionDiemThuongSuDung);
                }
                else
                {
                    HttpContext.Session.SetInt32(SessionDiemThuongSuDung, diemHopLe);
                }
            }

            return diemHopLe;
        }

        private static decimal TinhTongSauKhuyenMai(GioHang gioHang, KhuyenMai? khuyenMai)
        {
            var tongTien = gioHang.ChiTiet.Sum(c => c.SoLuong * (c.SanPham?.GiaBan ?? 0m));
            var tienGiam = khuyenMai == null ? 0m : Math.Round(tongTien * khuyenMai.PhanTramGiam / 100m, 0);
            return Math.Max(0m, tongTien - tienGiam);
        }

        private static void ApDungTienGiamTuDiem(List<ChiTietDonHang> chiTiet, decimal tienGiamTuDiem)
        {
            if (tienGiamTuDiem <= 0 || chiTiet.Count == 0) return;

            var tongConLai = chiTiet.Sum(item => item.SoLuong * item.DonGia - item.TienGiam);
            if (tongConLai <= 0) return;

            var tienCanGiam = Math.Min(tienGiamTuDiem, tongConLai);
            var daPhanBo = 0m;

            for (var i = 0; i < chiTiet.Count; i++)
            {
                var item = chiTiet[i];
                var thanhTienSauKhuyenMai = item.SoLuong * item.DonGia - item.TienGiam;
                var tienGiamThem = i == chiTiet.Count - 1
                    ? tienCanGiam - daPhanBo
                    : Math.Round(tienCanGiam * thanhTienSauKhuyenMai / tongConLai, 0);

                tienGiamThem = Math.Clamp(tienGiamThem, 0m, thanhTienSauKhuyenMai);
                item.TienGiam += tienGiamThem;
                daPhanBo += tienGiamThem;
            }
        }
    }
}
