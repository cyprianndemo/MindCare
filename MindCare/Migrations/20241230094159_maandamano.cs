using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class maandamano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PsychiatristId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_StudentId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "MedicationPrescription");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_PsychiatristId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_StudentId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ConsultationType",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "LicenseNumber",
                table: "AspNetUsers",
                newName: "StudentId");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PsychiatristId",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PrescribedDate",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Medications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "PrescriptionMedication",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    MedicationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionMedication", x => new { x.PrescriptionId, x.MedicationId });
                    table.ForeignKey(
                        name: "FK_PrescriptionMedication_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "MedicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrescriptionMedication_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicationId",
                table: "Prescriptions",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedication_MedicationId",
                table: "PrescriptionMedication",
                column: "MedicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Medications_MedicationId",
                table: "Prescriptions",
                column: "MedicationId",
                principalTable: "Medications",
                principalColumn: "MedicationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Medications_MedicationId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "PrescriptionMedication");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicationId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PrescribedDate",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "AspNetUsers",
                newName: "LicenseNumber");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Prescriptions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PsychiatristId",
                table: "Prescriptions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Medications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ConsultationType",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicationPrescription",
                columns: table => new
                {
                    MedicationsMedicationId = table.Column<int>(type: "integer", nullable: false),
                    PrescriptionsPrescriptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationPrescription", x => new { x.MedicationsMedicationId, x.PrescriptionsPrescriptionId });
                    table.ForeignKey(
                        name: "FK_MedicationPrescription_Medications_MedicationsMedicationId",
                        column: x => x.MedicationsMedicationId,
                        principalTable: "Medications",
                        principalColumn: "MedicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationPrescription_Prescriptions_PrescriptionsPrescript~",
                        column: x => x.PrescriptionsPrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PsychiatristId",
                table: "Prescriptions",
                column: "PsychiatristId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_StudentId",
                table: "Prescriptions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationPrescription_PrescriptionsPrescriptionId",
                table: "MedicationPrescription",
                column: "PrescriptionsPrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PsychiatristId",
                table: "Prescriptions",
                column: "PsychiatristId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_StudentId",
                table: "Prescriptions",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
