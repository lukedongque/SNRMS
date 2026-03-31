using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRotationAssignmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentRotationHistories",
                columns: table => new
                {
                    StudentRotationHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    RotationAssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRotationHistories", x => x.StudentRotationHistoryId);
                    table.ForeignKey(
                        name: "FK_StudentRotationHistories_RotationAssignments_RotationAssignm~",
                        column: x => x.RotationAssignmentId,
                        principalTable: "RotationAssignments",
                        principalColumn: "RotationAssignmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentRotationHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$tlNmKJBd68TQi.hB3nnYlO3mk3b2QIYZ7jcuAfoMpCq.eNScdsKkm");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRotationHistories_RotationAssignmentId",
                table: "StudentRotationHistories",
                column: "RotationAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRotationHistories_StudentId",
                table: "StudentRotationHistories",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentRotationHistories");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$4hHAFlxYKX/z25CrffDEQOKxs5cEQrAl2ExOSNzF8F7UO7p1y4jJ2");
        }
    }
}
