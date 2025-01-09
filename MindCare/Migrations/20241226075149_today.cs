using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class today : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Medications");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Medications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "Medications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Medications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Medications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Medications");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Medications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
