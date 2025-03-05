using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class appt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentFirstName",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "StudentLastName",
                table: "Appointments",
                newName: "StudentName");

            migrationBuilder.AddColumn<string>(
                name: "StudentEmail",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentEmail",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "StudentName",
                table: "Appointments",
                newName: "StudentLastName");

            migrationBuilder.AddColumn<string>(
                name: "StudentFirstName",
                table: "Appointments",
                type: "text",
                nullable: true);
        }
    }
}
