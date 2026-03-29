using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class IsActivetoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "IsActive", "PasswordHash" },
                values: new object[] { true, "$2a$11$9e3DJNFi/LzP4R0lm410veKOpd/tLg869wpUHbC8ZWMnLcPXAvJwa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$L/vnYnNU7Gh36I4nQ0JB9OYwCRVEH3RBPKQtarHGvD/6BIHefEFNe");
        }
    }
}
