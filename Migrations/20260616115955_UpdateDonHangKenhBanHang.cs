using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLministop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonHangKenhBanHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_NhanViens_NhanVienId",
                table: "DonHangs");

            migrationBuilder.AlterColumn<int>(
                name: "NhanVienId",
                table: "DonHangs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "KenhBanHang",
                table: "DonHangs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_NhanViens_NhanVienId",
                table: "DonHangs",
                column: "NhanVienId",
                principalTable: "NhanViens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_NhanViens_NhanVienId",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "KenhBanHang",
                table: "DonHangs");

            migrationBuilder.AlterColumn<int>(
                name: "NhanVienId",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_NhanViens_NhanVienId",
                table: "DonHangs",
                column: "NhanVienId",
                principalTable: "NhanViens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
