using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Classroom.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsOrTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HomeworkUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Point",
                table: "HomeworkUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Work",
                table: "HomeworkUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "ClassUser",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HomeworkUser");

            migrationBuilder.DropColumn(
                name: "Point",
                table: "HomeworkUser");

            migrationBuilder.DropColumn(
                name: "Work",
                table: "HomeworkUser");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "ClassUser");
        }
    }
}
