using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebQLministop.Data;

namespace WebQLministop.Controllers;

[ApiController]
[Route("api/sepay")]
public class SePayWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SePayWebhookController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        if (!KiemTraBaoMat(body)) return Unauthorized(new { success = false });

        var payload = JsonSerializer.Deserialize<SePayWebhookPayload>(body, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (payload == null) return BadRequest(new { success = false });

        var maDonHang = LayMaDonHang(payload.Content, payload.Description, payload.Code);
        if (maDonHang == null || payload.TransferAmount <= 0)
        {
            return Ok(new { success = true });
        }

        var donHang = await _context.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet)
            .ThenInclude(c => c.SanPham)
            .FirstOrDefaultAsync(d =>
                d.Id == maDonHang.Value &&
                d.PhuongThucThanhToan == "ChuyenKhoan" &&
                d.TrangThai == "DangXuLy");

        if (donHang == null || payload.TransferAmount < donHang.TongTien)
        {
            return Ok(new { success = true });
        }

        if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(payload.TransferType))
        {
            return Ok(new { success = true });
        }

        if (donHang.NgayDat.AddMinutes(5) < DateTime.UtcNow)
        {
            donHang.TrangThai = "DaHuy";
            donHang.GhiChuThanhToan = $"{donHang.GhiChuThanhToan} SePAY báo giao dịch sau khi đơn đã quá hạn 5 phút nên đơn không được xác nhận.";
            foreach (var item in donHang.ChiTiet)
            {
                if (item.SanPham != null)
                {
                    item.SanPham.TonKho += item.SoLuong;
                }
            }
            if (donHang.KhachHang != null && donHang.DiemThuongSuDung > 0)
            {
                donHang.KhachHang.DiemThuong += donHang.DiemThuongSuDung;
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        donHang.TrangThai = "DaThanhToan";
        donHang.GhiChuThanhToan = $"{donHang.GhiChuThanhToan} SePAY đã xác nhận nhận tiền: {payload.TransferAmount:N0}đ.";
        if (donHang.KhachHang != null && donHang.DiemThuongCong > 0)
        {
            donHang.KhachHang.DiemThuong += donHang.DiemThuongCong;
        }

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private bool KiemTraBaoMat(string body)
    {
        // Tạm thời tắt bảo mật theo yêu cầu của user để test dễ dàng hơn
        return true;
    }

    private static int? LayMaDonHang(params string?[] values)
    {
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;

            var match = Regex.Match(value, @"(?:DH|HD)[\s-]*0*(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
            {
                return id;
            }

            match = Regex.Match(value, @"#\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out id))
            {
                return id;
            }
        }

        return null;
    }
}

public class SePayWebhookPayload
{
    [JsonPropertyName("transferType")]
    public string? TransferType { get; set; }

    [JsonPropertyName("transferAmount")]
    public decimal TransferAmount { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
