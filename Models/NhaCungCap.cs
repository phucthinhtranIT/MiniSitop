using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class NhaCungCap
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Ten { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TenLienHe { get; set; }

    [StringLength(20)]
    public string? DienThoai { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? DiaChi { get; set; }
}
