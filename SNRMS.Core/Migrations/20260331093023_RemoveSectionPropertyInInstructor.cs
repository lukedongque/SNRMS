using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSectionPropertyInInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Sections_SectionId1",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_SectionId1",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "SectionId1",
                table: "Instructors");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.8LeKkSnRNxM7T4VcCHYtud0qDpozMRy800VLwtwgcZ0WG7R0nlnO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Instructors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionId1",
                table: "Instructors",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$tlNmKJBd68TQi.hB3nnYlO3mk3b2QIYZ7jcuAfoMpCq.eNScdsKkm");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_SectionId1",
                table: "Instructors",
                column: "SectionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Sections_SectionId1",
                table: "Instructors",
                column: "SectionId1",
                principalTable: "Sections",
                principalColumn: "SectionId");
        }
    }
}
