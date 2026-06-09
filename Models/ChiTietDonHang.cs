using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class ChiTietDonHang
{
    public int Id { get; set; }

    public int DonHangId { get; set; }
    public DonHang? DonHang { get; set; }

    public int SanPhamId { get; set; }
    public SanPham? SanPham { get; set; }

    public int SoLuong { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DonGia { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TienGiam { get; set; }
}
