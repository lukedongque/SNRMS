using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigationoInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_InstructorId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Vnl1tekGm0BaGzYavzcUrO6Gxu20XOVryDtuH8R3T6P7hfv2ZvkqS");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstructorId",
                table: "Users",
                column: "InstructorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_InstructorId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$p.RbB5OBg.kXYcgRKa8zNenEBkCG8zzD2gthQflabRIZHee3IikMC");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstructorId",
                table: "Users",
                column: "InstructorId");
        }
    }
}
