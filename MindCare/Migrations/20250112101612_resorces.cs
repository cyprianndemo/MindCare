using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class resorces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "MentalHealthResources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "MentalHealthResources",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "MentalHealthResources",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "MentalHealthResources",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "MentalHealthResources");
        }
    }
}
