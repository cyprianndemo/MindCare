using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class mood3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "MoodEntries");

            migrationBuilder.DropColumn(
                name: "MoodScore",
                table: "MoodEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MoodEntries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MoodEntries_AspNetUsers_UserId",
                table: "MoodEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoodEntries_AspNetUsers_UserId",
                table: "MoodEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MoodEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "MoodEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MoodScore",
                table: "MoodEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
