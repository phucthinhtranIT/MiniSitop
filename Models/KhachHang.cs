using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class KhachHang
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(20)]
    public string? DienThoai { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    public int DiemThuong { get; set; }

    public bool KichHoat { get; set; } = true;
}
