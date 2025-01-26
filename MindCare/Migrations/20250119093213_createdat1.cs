using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class createdat1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PsychiatristId",
                table: "Prescriptions",
                column: "PsychiatristId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PsychiatristId",
                table: "Prescriptions",
                column: "PsychiatristId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PsychiatristId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_PsychiatristId",
                table: "Prescriptions");
        }
    }
}
