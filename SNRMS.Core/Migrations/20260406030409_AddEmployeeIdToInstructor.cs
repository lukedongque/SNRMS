using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIdToInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "Instructors",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$p.RbB5OBg.kXYcgRKa8zNenEBkCG8zzD2gthQflabRIZHee3IikMC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Instructors");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.8LeKkSnRNxM7T4VcCHYtud0qDpozMRy800VLwtwgcZ0WG7R0nlnO");
        }
    }
}
