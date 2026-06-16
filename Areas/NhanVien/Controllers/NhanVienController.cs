using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using WebQLministop.Data;
using WebQLministop.Models;
using KhachHangModel = WebQLministop.Models.KhachHang;

namespace WebQLministop.Areas.NhanVien.Controllers;

[Area("NhanVien")]
public class NhanVienController : Controller
{
    private readonly ApplicationDbContext _context;

    public NhanVienController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!LaNhanVienHoacQuanLy())
        {
            return RedirectToAction("Index", "DangNhap", new { area = "KhachHang" });
        }

        ViewBag.SanPhams = await _context.SanPhams
            .Include(s => s.DanhMuc)
            .Where(s => s.KichHoat)
            .OrderBy(s => s.Ten)
            .ToListAsync();

        ViewBag.KhachHangs = await _context.KhachHangs
            .Where(k => k.KichHoat && k.HoTen != "Khach le")
            .OrderBy(k => k.HoTen)
            .ToListAsync();

        ViewBag.NhanViens = await _context.NhanViens
            .Where(n => n.KichHoat)
            .OrderBy(n => n.HoTen)
            .ToListAsync();

        ViewBag.DonHangs = await _context.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.NhanVien)
            .OrderByDescending(d => d.NgayDat)
            .Take(10)
            .ToListAsync();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TimSanPham(string? tuKhoa)
    {
        if (!LaNhanVienHoacQuanLy())
        {
            return Unauthorized();
        }

        var keyword = (tuKhoa ?? string.Empty).Trim();
        if (keyword.Length == 0)
        {
            return Json(Array.Empty<object>());
        }

        var sanPhams = await _context.SanPhams
            .Include(s => s.DanhMuc)
            .Where(s => s.KichHoat)
            .OrderBy(s => s.Ten)
            .ToListAsync();

        var keywordKhongDau = BoDau(keyword);
        var ketQua = sanPhams
            .Where(s => BoDau(s.Ma).Contains(keywordKhongDau) || BoDau(s.Ten).Contains(keywordKhongDau))
            .Take(10)
            .Select(s => new
            {
                s.Id,
                s.Ma,
                s.Ten,
                s.GiaBan,
                s.TonKho,
                s.DonVi,
                HinhAnh = string.IsNullOrWhiteSpace(s.HinhAnh) ? "https://images.unsplash.com/photo-1601598851547-4302969d0614?auto=format&fit=crop&w=600&q=80" : s.HinhAnh,
                DanhMuc = HienThiDanhMuc(s.DanhMuc?.Ten)
            })
            .ToList();

        return Json(ketQua);
    }

    [HttpGet]
    public async Task<IActionResult> TimKhachHang(string? ma)
    {
        if (!LaNhanVienHoacQuanLy()) return Unauthorized();

        var search = (ma ?? "").Trim();
        if (string.IsNullOrEmpty(search)) return Json(new { thanhCong = false, thongBao = "Vui lòng nhập mã." });

        // Tách số từ mã "KH-0000" hoặc "0000"
        var parsedId = search.Replace("KH-", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (int.TryParse(parsedId, out int khId))
        {
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khId && k.KichHoat);
            if (khachHang != null)
            {
                return Json(new { thanhCong = true, id = khachHang.Id, hoTen = khachHang.HoTen, diemThuong = khachHang.DiemThuong });
            }
        }

        return Json(new { thanhCong = false, thongBao = "Không tìm thấy khách hàng." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoDonTaiQuay([FromBody] TaoDonTaiQuayRequest request)
    {
        if (!LaNhanVienHoacQuanLy())
        {
            return Unauthorized(new { thanhCong = false, thongBao = "Vui lòng đăng nhập bằng tài khoản nhân viên." });
        }

        if (request.SanPhams.Count == 0)
        {
            return BadRequest(new { thanhCong = false, thongBao = "Hóa đơn chưa có sản phẩm." });
        }

        var sanPhamIds = request.SanPhams.Select(i => i.SanPhamId).Distinct().ToList();
        var sanPhams = await _context.SanPhams
            .Where(s => sanPhamIds.Contains(s.Id) && s.KichHoat)
            .ToDictionaryAsync(s => s.Id);

        var chiTietDonHang = new List<ChiTietDonHang>();
        foreach (var item in request.SanPhams)
        {
            if (item.SoLuong <= 0)
            {
                return BadRequest(new { thanhCong = false, thongBao = "Số lượng sản phẩm không hợp lệ." });
            }

            if (!sanPhams.TryGetValue(item.SanPhamId, out var sanPham))
            {
                return BadRequest(new { thanhCong = false, thongBao = "Có sản phẩm không còn hoạt động." });
            }

            if (sanPham.TonKho < item.SoLuong)
            {
                return BadRequest(new { thanhCong = false, thongBao = $"{sanPham.Ten} chỉ còn {sanPham.TonKho} {sanPham.DonVi}." });
            }

            sanPham.TonKho -= item.SoLuong;
            chiTietDonHang.Add(new ChiTietDonHang
            {
                SanPhamId = sanPham.Id,
                SoLuong = item.SoLuong,
                DonGia = sanPham.GiaBan,
                TienGiam = 0m
            });
        }

        var nhanVienId = await LayNhanVienLapDon(request.NhanVienId);
        if (nhanVienId == null)
        {
            return BadRequest(new { thanhCong = false, thongBao = "Không tìm thấy nhân viên lập hóa đơn." });
        }

        var khachHang = await LayKhachHangChoDonTaiQuay(request.KhachHangId);
        var tongTienGoc = chiTietDonHang.Sum(i => i.SoLuong * i.DonGia - i.TienGiam);
        
        // Xử lý điểm sử dụng
        var tienGiamTuDiem = 0m;
        if (request.DiemSuDung > 0 && khachHang.HoTen != "Khach le")
        {
            if (khachHang.DiemThuong < request.DiemSuDung)
            {
                return BadRequest(new { thanhCong = false, thongBao = "Không đủ điểm tích lũy." });
            }
            
            tienGiamTuDiem = request.DiemSuDung * 1m; // 1 điểm = 1 vnđ
            if (tienGiamTuDiem > tongTienGoc)
            {
                tienGiamTuDiem = tongTienGoc;
                request.DiemSuDung = (int)tongTienGoc;
            }
            
            khachHang.DiemThuong -= request.DiemSuDung;
        }

        var tongSauGiam = tongTienGoc - tienGiamTuDiem;
        var diemCong = khachHang.HoTen == "Khach le" ? 0 : TinhDiemCong(tongSauGiam);

        if (diemCong > 0)
        {
            khachHang.DiemThuong += diemCong;
        }

        var phuongThuc = string.Equals(request.PhuongThucThanhToan, "ChuyenKhoan", StringComparison.OrdinalIgnoreCase) 
            ? "ChuyenKhoan" 
            : "TienMat";
        var trangThai = phuongThuc == "ChuyenKhoan" ? "DangXuLy" : "DaThanhToan";

        var donHang = new DonHang
        {
            KhachHangId = khachHang.Id,
            NhanVienId = nhanVienId.Value,
            KenhBanHang = "TaiQuay",
            DiaChiGiaoHang = "Tại quầy",
            NgayDat = DateTime.UtcNow,
            TrangThai = trangThai,
            PhuongThucThanhToan = phuongThuc,
            TongTien = tongSauGiam,
            DiemThuongSuDung = request.DiemSuDung,
            DiemThuongCong = diemCong,
            ChiTiet = chiTietDonHang
        };

        _context.DonHangs.Add(donHang);
        await _context.SaveChangesAsync();

        return Json(new
        {
            thanhCong = true,
            maDonHang = donHang.Id,
            tongTien = donHang.TongTien,
            diemCong,
            thongBao = diemCong > 0
                ? $"Đã tạo hóa đơn #{donHang.Id}. Khách được cộng {diemCong:N0} điểm tích lũy."
                : $"Đã tạo hóa đơn #{donHang.Id}."
        });
    }

    private async Task<int?> LayNhanVienLapDon(int? nhanVienIdDuocChon)
    {
        if (nhanVienIdDuocChon != null &&
            await _context.NhanViens.AnyAsync(n => n.Id == nhanVienIdDuocChon.Value && n.KichHoat))
        {
            return nhanVienIdDuocChon.Value;
        }

        var nhanVienIdSession = HttpContext.Session.GetInt32("NhanVienId");
        if (nhanVienIdSession != null &&
            await _context.NhanViens.AnyAsync(n => n.Id == nhanVienIdSession.Value && n.KichHoat))
        {
            return nhanVienIdSession.Value;
        }

        return await _context.NhanViens
            .Where(n => n.KichHoat)
            .OrderBy(n => n.Id)
            .Select(n => (int?)n.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<KhachHangModel> LayKhachHangChoDonTaiQuay(int? khachHangId)
    {
        if (khachHangId != null)
        {
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Id == khachHangId.Value && k.KichHoat);
            if (khachHang != null)
            {
                return khachHang;
            }
        }

        var khachLe = await _context.KhachHangs.FirstOrDefaultAsync(k =>
            k.HoTen == "Khach le" &&
            k.Email == null &&
            k.DienThoai == null);

        if (khachLe != null)
        {
            return khachLe;
        }

        khachLe = new KhachHangModel
        {
            HoTen = "Khach le",
            KichHoat = true
        };
        _context.KhachHangs.Add(khachLe);
        await _context.SaveChangesAsync();
        return khachLe;
    }

    private static int TinhDiemCong(decimal tongTienGoc)
    {
        return (int)Math.Floor(tongTienGoc / 100m);
    }

    private static string HienThiDanhMuc(string? ten)
    {
        return ten switch
        {
            "Do uong" => "Đồ uống",
            "Banh keo" => "Bánh kẹo",
            "Thuc pham" => "Thực phẩm",
            "Gia dung" => "Gia dụng",
            _ => string.IsNullOrWhiteSpace(ten) ? "Khác" : ten
        };
    }

    private bool LaNhanVienHoacQuanLy()
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        return vaiTro == "NhanVien" || vaiTro == "QuanLy";
    }

    [HttpGet]
    public async Task<IActionResult> InHoaDon(int id)
    {
        if (!LaNhanVienHoacQuanLy()) return Unauthorized();

        var donHang = await _context.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.NhanVien)
            .Include(d => d.ChiTiet)
                .ThenInclude(c => c.SanPham)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donHang == null)
        {
            return NotFound("Không tìm thấy hóa đơn.");
        }

        return View(donHang);
    }

    private static string BoDau(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd');
    }
}

public class TaoDonTaiQuayRequest
{
    public int? KhachHangId { get; set; }
    public int? NhanVienId { get; set; }
    public string? PhuongThucThanhToan { get; set; }
    public int DiemSuDung { get; set; }
    public List<TaoDonTaiQuayItem> SanPhams { get; set; } = new();
}

public class TaoDonTaiQuayItem
{
    public int SanPhamId { get; set; }
    public int SoLuong { get; set; }
}
