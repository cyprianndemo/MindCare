using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class resorces3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scenario",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "Tags",
                table: "MentalHealthResources",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Urgency",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scenario",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "MentalHealthResources");
        }
    }
}
