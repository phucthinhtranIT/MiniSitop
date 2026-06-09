using Microsoft.EntityFrameworkCore;
using WebQLministop.Models;

namespace WebQLministop.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<DanhMuc> DanhMucs => Set<DanhMuc>();
    public DbSet<NhaCungCap> NhaCungCaps => Set<NhaCungCap>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
    public DbSet<DonHang> DonHangs => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NhanVien>().Property(e => e.Luong).HasPrecision(18, 2);
        modelBuilder.Entity<DonHang>().Property(o => o.TongTien).HasPrecision(18, 2);
        modelBuilder.Entity<ChiTietDonHang>().Property(oi => oi.DonGia).HasPrecision(18, 2);
        modelBuilder.Entity<ChiTietDonHang>().Property(oi => oi.TienGiam).HasPrecision(18, 2);
        modelBuilder.Entity<SanPham>().Property(p => p.GiaVon).HasPrecision(18, 2);
        modelBuilder.Entity<SanPham>().Property(p => p.GiaBan).HasPrecision(18, 2);
        modelBuilder.Entity<KhuyenMai>().Property(p => p.PhanTramGiam).HasPrecision(5, 2);

        modelBuilder.Entity<SanPham>()
            .HasOne(p => p.DanhMuc)
            .WithMany()
            .HasForeignKey(p => p.DanhMucId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SanPham>()
            .HasOne(p => p.NhaCungCap)
            .WithMany()
            .HasForeignKey(p => p.NhaCungCapId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChiTietDonHang>()
            .HasOne(oi => oi.DonHang)
            .WithMany(o => o.ChiTiet)
            .HasForeignKey(oi => oi.DonHangId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietDonHang>()
            .HasOne(oi => oi.SanPham)
            .WithMany()
            .HasForeignKey(oi => oi.SanPhamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
