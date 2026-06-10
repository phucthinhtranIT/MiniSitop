using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class PhanCaNhanVien
{
    public int Id { get; set; }

    public int CaLamViecId { get; set; }
    public CaLamViec? CaLamViec { get; set; }

    public int NhanVienId { get; set; }
    public NhanVien? NhanVien { get; set; }

    public DateTime NgayLam { get; set; } = DateTime.Today;

    [StringLength(250)]
    public string? GhiChu { get; set; }
}
