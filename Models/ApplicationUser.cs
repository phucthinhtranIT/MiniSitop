using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebQLministop.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(30)]
    public string LoaiTaiKhoan { get; set; } = "KhachHang";

    public int? KhachHangId { get; set; }

    public KhachHang? KhachHang { get; set; }

    public int? NhanVienId { get; set; }

    public NhanVien? NhanVien { get; set; }
}
