using System.ComponentModel.DataAnnotations;

namespace WebQLministop.Models;

public class KhuyenMai
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Ten { get; set; } = string.Empty;

    [StringLength(50)]
    public string Ma { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal PhanTramGiam { get; set; }

    public DateTime NgayBatDau { get; set; } = DateTime.UtcNow;

    public DateTime NgayKetThuc { get; set; } = DateTime.UtcNow.AddDays(7);

    public bool KichHoat { get; set; } = true;
}
