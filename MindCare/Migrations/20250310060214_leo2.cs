using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class leo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "UserActivities",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "UserActivities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityId",
                table: "UserActivities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "UserActivities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserActivities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserActivities");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserActivities",
                newName: "ActivityId");
        }
    }
}
