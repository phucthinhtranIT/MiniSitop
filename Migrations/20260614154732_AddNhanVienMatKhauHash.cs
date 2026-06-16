using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLministop.Migrations
{
    /// <inheritdoc />
    public partial class AddNhanVienMatKhauHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatKhauHash",
                table: "NhanViens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatKhauHash",
                table: "NhanViens");
        }
    }
}
