using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class SanPham
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Ma { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Ten { get; set; } = string.Empty;

    [StringLength(250)]
    public string? MoTa { get; set; }

    public int DanhMucId { get; set; }
    public DanhMuc? DanhMuc { get; set; }

    public int NhaCungCapId { get; set; }
    public NhaCungCap? NhaCungCap { get; set; }

    [Range(0, double.MaxValue)]
    public decimal GiaVon { get; set; }

    [Range(0, double.MaxValue)]
    public decimal GiaBan { get; set; }

    [StringLength(30)]
    public string DonVi { get; set; } = "pcs";

    public int TonKho { get; set; }

    public int MucCanNhapLai { get; set; }

    public bool KichHoat { get; set; } = true;
}
