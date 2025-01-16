using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class mood1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrescriptionDate",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_StudentId",
                table: "Prescriptions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_StudentId_EntryDate",
                table: "MoodEntries",
                columns: new[] { "StudentId", "EntryDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_StudentId",
                table: "Prescriptions",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_StudentId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_StudentId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_MoodEntries_StudentId_EntryDate",
                table: "MoodEntries");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PrescriptionDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Prescriptions");
        }
    }
}
