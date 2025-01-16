using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class resorces1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MentalHealthResources_MentalHealthResourceResou~",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MentalHealthResourceResourceId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "MentalHealthResources");

            migrationBuilder.DropColumn(
                name: "MentalHealthResourceResourceId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "MentalHealthResources",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "IsOnline",
                table: "MentalHealthResources",
                newName: "IsDownloadable");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "MentalHealthResources",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MentalHealthResources",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "MentalHealthResources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "MentalHealthResources");

            migrationBuilder.RenameColumn(
                name: "ResourceType",
                table: "MentalHealthResources",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "IsDownloadable",
                table: "MentalHealthResources",
                newName: "IsOnline");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MentalHealthResources",
                newName: "ResourceId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "MentalHealthResources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "MentalHealthResources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MentalHealthResources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MentalHealthResources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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
                name: "Language",
                table: "MentalHealthResources",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MentalHealthResourceResourceId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MentalHealthResourceResourceId",
                table: "AspNetUsers",
                column: "MentalHealthResourceResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MentalHealthResources_MentalHealthResourceResou~",
                table: "AspNetUsers",
                column: "MentalHealthResourceResourceId",
                principalTable: "MentalHealthResources",
                principalColumn: "ResourceId");
        }
    }
}
