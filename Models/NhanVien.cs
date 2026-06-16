using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class NhanVien
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(50)]
    public string ChucVu { get; set; } = string.Empty;

    [StringLength(20)]
    public string? DienThoai { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? MatKhauHash { get; set; }

    public decimal Luong { get; set; }

    public DateTime NgayVaoLam { get; set; } = DateTime.UtcNow;

    public bool KichHoat { get; set; } = true;
}
