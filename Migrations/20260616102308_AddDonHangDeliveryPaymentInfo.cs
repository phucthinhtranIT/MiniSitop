using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLministop.Migrations
{
    /// <inheritdoc />
    public partial class AddDonHangDeliveryPaymentInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaChiGiaoHang",
                table: "DonHangs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChuThanhToan",
                table: "DonHangs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChiGiaoHang",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "GhiChuThanhToan",
                table: "DonHangs");
        }
    }
}
