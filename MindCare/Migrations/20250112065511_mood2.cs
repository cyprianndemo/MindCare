using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class mood2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MoodEntries_StudentId_EntryDate",
                table: "MoodEntries");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "MoodEntries");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MoodEntries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_UserId_EntryDate",
                table: "MoodEntries",
                columns: new[] { "UserId", "EntryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MoodEntries_UserId_EntryDate",
                table: "MoodEntries");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MoodEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "MoodEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_StudentId_EntryDate",
                table: "MoodEntries",
                columns: new[] { "StudentId", "EntryDate" });
        }
    }
}
