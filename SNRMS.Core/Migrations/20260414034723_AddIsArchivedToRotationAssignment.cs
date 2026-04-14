using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToRotationAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "RotationAssignments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$cHNmRs/PeOu7vVmB3mPzZ.z7A5qjywR2E2DpjGC6mzHW0BPH9rJqi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "RotationAssignments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RD3Dz1icXPu9lh7vkLeGOu0igLS307jfIc2oP.fvjbsSNpjzCjcKm");
        }
    }
}
