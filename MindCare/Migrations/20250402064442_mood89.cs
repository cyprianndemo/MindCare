using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class mood89 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoodScore",
                table: "MoodEntries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoodScore",
                table: "MoodEntries");
        }
    }
}
