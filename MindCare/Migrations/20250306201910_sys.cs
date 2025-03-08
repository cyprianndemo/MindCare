using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class sys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupportEmail = table.Column<string>(type: "text", nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "boolean", nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxAppointmentsPerDay = table.Column<int>(type: "integer", nullable: false),
                    AllowSelfRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledPaymentMethods = table.Column<string>(type: "text", nullable: false),
                    VideoCallProvider = table.Column<string>(type: "text", nullable: false),
                    DefaultCurrencySymbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SystemTimeZone = table.Column<string>(type: "text", nullable: false),
                    NotificationSettings = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
