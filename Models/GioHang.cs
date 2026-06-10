namespace WebQLministop.Models;

public class GioHang
{
    public int Id { get; set; }

    public int KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }

    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    public List<ChiTietGioHang> ChiTiet { get; set; } = new();
}
