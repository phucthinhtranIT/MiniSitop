using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLministop.Migrations
{
    /// <inheritdoc />
    public partial class AddDonHangRewardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiemThuongCong",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiemThuongSuDung",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaKhuyenMaiDaDung",
                table: "DonHangs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TienGiamDiem",
                table: "DonHangs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TienGiamKhuyenMai",
                table: "DonHangs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemThuongCong",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "DiemThuongSuDung",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "MaKhuyenMaiDaDung",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "TienGiamDiem",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "TienGiamKhuyenMai",
                table: "DonHangs");
        }
    }
}
