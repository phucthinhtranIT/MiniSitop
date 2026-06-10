using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class CaLamViec
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string TenCa { get; set; } = string.Empty;

    public TimeSpan GioBatDau { get; set; }

    public TimeSpan GioKetThuc { get; set; }

    [StringLength(250)]
    public string? MoTa { get; set; }

    public List<PhanCaNhanVien> PhanCaNhanViens { get; set; } = new();
}
