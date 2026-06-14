using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class DonHang
{
    public int Id { get; set; }

    public int KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }

    public int NhanVienId { get; set; }
    public NhanVien? NhanVien { get; set; }

    public DateTime NgayDat { get; set; } = DateTime.UtcNow;

    [StringLength(30)]
    public string TrangThai { get; set; } = "DaThanhToan";

    [StringLength(30)]
    public string PhuongThucThanhToan { get; set; } = "TienMat";

    [Range(0, double.MaxValue)]
    public decimal TongTien { get; set; }

    public int DiemThuongSuDung { get; set; }

    public int DiemThuongCong { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TienGiamDiem { get; set; }

    [StringLength(50)]
    public string? MaKhuyenMaiDaDung { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TienGiamKhuyenMai { get; set; }

    public List<ChiTietDonHang> ChiTiet { get; set; } = new();
}
