using Microsoft.EntityFrameworkCore;
using WebQLministop.Data;
using WebQLministop.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=.\\SQLEXPRESS;Database=WebQLministop;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    SeedData(db);
}

app.Run();

static void SeedData(ApplicationDbContext db)
{
    if (db.DanhMucs.Any()) return;

    var categories = new[]
    {
        new DanhMuc { Ten = "Do uong", MoTa = "Nuoc ngot, nuoc suoi, ca phe" },
        new DanhMuc { Ten = "Banh keo", MoTa = "Snack, keo, banh" },
        new DanhMuc { Ten = "Thuc pham", MoTa = "Mi goi, sua, do an nhanh" },
        new DanhMuc { Ten = "Gia dung", MoTa = "Ve sinh, hop dung" }
    };
    db.DanhMucs.AddRange(categories);

    var suppliers = new[]
    {
        new NhaCungCap { Ten = "Cong ty ABC", TenLienHe = "Nguyen Van A", DienThoai = "0901111111", Email = "abc@ministop.vn", DiaChi = "Quan 1" },
        new NhaCungCap { Ten = "Cong ty Tan Phat", TenLienHe = "Tran Thi B", DienThoai = "0902222222", Email = "tanphat@ministop.vn", DiaChi = "Quan 7" },
        new NhaCungCap { Ten = "Cong ty Minh Anh", TenLienHe = "Le Van C", DienThoai = "0903333333", Email = "minhanh@ministop.vn", DiaChi = "Binh Thanh" }
    };
    db.NhaCungCaps.AddRange(suppliers);
    db.SaveChanges();

    var products = new[]
    {
        new SanPham { Ma = "DR001", Ten = "Coca Cola 330ml", DanhMucId = categories[0].Id, NhaCungCapId = suppliers[0].Id, GiaVon = 7_000m, GiaBan = 10_000m, DonVi = "chai", TonKho = 120, MucCanNhapLai = 30 },
        new SanPham { Ma = "DR002", Ten = "Pepsi 355ml", DanhMucId = categories[0].Id, NhaCungCapId = suppliers[1].Id, GiaVon = 7_500m, GiaBan = 10_500m, DonVi = "chai", TonKho = 100, MucCanNhapLai = 25 },
        new SanPham { Ma = "SN001", Ten = "Snack Oishi", DanhMucId = categories[1].Id, NhaCungCapId = suppliers[2].Id, GiaVon = 5_000m, GiaBan = 7_500m, DonVi = "goi", TonKho = 200, MucCanNhapLai = 40 },
        new SanPham { Ma = "FD001", Ten = "Mi Omachi", DanhMucId = categories[2].Id, NhaCungCapId = suppliers[0].Id, GiaVon = 4_000m, GiaBan = 6_000m, DonVi = "goi", TonKho = 150, MucCanNhapLai = 30 },
        new SanPham { Ma = "HM001", Ten = "Khăn giấy 4 lop", DanhMucId = categories[3].Id, NhaCungCapId = suppliers[1].Id, GiaVon = 12_000m, GiaBan = 16_000m, DonVi = "cuon", TonKho = 80, MucCanNhapLai = 20 }
    };
    db.SanPhams.AddRange(products);

    var employees = new[]
    {
        new NhanVien { HoTen = "Nguyen Thi Lan", ChucVu = "Quan ly", DienThoai = "0910000001", Email = "lan@ministop.vn", Luong = 15000000m },
        new NhanVien { HoTen = "Tran Minh Tuan", ChucVu = "Thu ngan", DienThoai = "0910000002", Email = "tuan@ministop.vn", Luong = 9000000m },
        new NhanVien { HoTen = "Pham Hong Nhung", ChucVu = "Pha che", DienThoai = "0910000003", Email = "nhung@ministop.vn", Luong = 8500000m }
    };
    db.NhanViens.AddRange(employees);

    var customers = new[]
    {
        new KhachHang { HoTen = "Le Van Dung", DienThoai = "0930000001", Email = "dung@gmail.com", DiemThuong = 120 },
        new KhachHang { HoTen = "Bui Thi Hanh", DienThoai = "0930000002", Email = "hanh@gmail.com", DiemThuong = 85 },
        new KhachHang { HoTen = "Do Minh Khoi", DienThoai = "0930000003", Email = "khoi@gmail.com", DiemThuong = 60 }
    };
    db.KhachHangs.AddRange(customers);

    var promotions = new[]
    {
        new KhuyenMai { Ten = "Khuyen mai cuoi tuan", PhanTramGiam = 10m, NgayBatDau = DateTime.UtcNow.AddDays(-5), NgayKetThuc = DateTime.UtcNow.AddDays(10) },
        new KhuyenMai { Ten = "Mua 2 tang 1", PhanTramGiam = 15m, NgayBatDau = DateTime.UtcNow, NgayKetThuc = DateTime.UtcNow.AddDays(20) }
    };
    db.KhuyenMais.AddRange(promotions);
    db.SaveChanges();

    var order = new DonHang
    {
        KhachHangId = customers[0].Id,
        NhanVienId = employees[1].Id,
        NgayDat = DateTime.UtcNow.AddDays(-1),
        TrangThai = "DaThanhToan",
        PhuongThucThanhToan = "TienMat",
        TongTien = 0m,
        ChiTiet = new List<ChiTietDonHang>
        {
            new ChiTietDonHang { SanPhamId = products[0].Id, SoLuong = 2, DonGia = products[0].GiaBan, TienGiam = 0m },
            new ChiTietDonHang { SanPhamId = products[2].Id, SoLuong = 1, DonGia = products[2].GiaBan, TienGiam = 0m }
        }
    };
    db.DonHangs.Add(order);
    db.SaveChanges();

    order.TongTien = order.ChiTiet.Sum(i => i.SoLuong * i.DonGia - i.TienGiam);
    db.DonHangs.Update(order);
    db.SaveChanges();
}
