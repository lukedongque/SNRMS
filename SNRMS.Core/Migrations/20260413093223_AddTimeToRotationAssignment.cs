using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNRMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeToRotationAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "RotationAssignments",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "RotationAssignments",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RD3Dz1icXPu9lh7vkLeGOu0igLS307jfIc2oP.fvjbsSNpjzCjcKm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "RotationAssignments");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "RotationAssignments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Vnl1tekGm0BaGzYavzcUrO6Gxu20XOVryDtuH8R3T6P7hfv2ZvkqS");
        }
    }
}
