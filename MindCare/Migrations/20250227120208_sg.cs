using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MindCare.Migrations
{
    /// <inheritdoc />
    public partial class sg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SupportGroups_SupportGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_SupportGroups_SupportGroupId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessages_AspNetUsers_ReceiverId",
                table: "DirectMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportGroups",
                table: "SupportGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DirectMessages",
                table: "DirectMessages");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SupportGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SupportGroupId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "DirectMessages",
                newName: "RecipientId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DirectMessages",
                newName: "ConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessages_ReceiverId",
                table: "DirectMessages",
                newName: "IX_DirectMessages_RecipientId");

            migrationBuilder.RenameColumn(
                name: "SupportGroupId",
                table: "ChatMessages",
                newName: "SupportGroupGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SupportGroupId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SupportGroupGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SupportGroups",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SupportGroups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "SupportGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SupportGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ConversationId",
                table: "DirectMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "DirectMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DirectMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CurrentMood",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMoodIcon",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportGroups",
                table: "SupportGroups",
                column: "GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DirectMessages",
                table: "DirectMessages",
                column: "MessageId");

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User1Id = table.Column<string>(type: "text", nullable: false),
                    User2Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_User1Id",
                        column: x => x.User1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_User2Id",
                        column: x => x.User2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discussions",
                columns: table => new
                {
                    DiscussionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discussions", x => x.DiscussionId);
                    table.ForeignKey(
                        name: "FK_Discussions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discussions_SupportGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SupportGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_GroupMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_SupportGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SupportGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentalHealthProfiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Biography = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false),
                    JourneyProgress = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalHealthProfiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_MentalHealthProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoodLogs",
                columns: table => new
                {
                    MoodLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MoodRating = table.Column<int>(type: "integer", nullable: false),
                    MoodDescription = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodLogs", x => x.MoodLogId);
                    table.ForeignKey(
                        name: "FK_MoodLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscussionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_Responses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Responses_Discussions_DiscussionId",
                        column: x => x.DiscussionId,
                        principalTable: "Discussions",
                        principalColumn: "DiscussionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_ConversationId",
                table: "DirectMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1Id",
                table: "Conversations",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id",
                table: "Conversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_GroupId",
                table: "Discussions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_UserId",
                table: "Discussions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentalHealthProfiles_UserId",
                table: "MentalHealthProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodLogs_UserId",
                table: "MoodLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_DiscussionId",
                table: "Responses",
                column: "DiscussionId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_UserId",
                table: "Responses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_SupportGroups_SupportGroupGroupId",
                table: "ChatMessages",
                column: "SupportGroupGroupId",
                principalTable: "SupportGroups",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessages_AspNetUsers_RecipientId",
                table: "DirectMessages",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessages_Conversations_ConversationId",
                table: "DirectMessages",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_SupportGroups_SupportGroupGroupId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessages_AspNetUsers_RecipientId",
                table: "DirectMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessages_Conversations_ConversationId",
                table: "DirectMessages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "MentalHealthProfiles");

            migrationBuilder.DropTable(
                name: "MoodLogs");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "Discussions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportGroups",
                table: "SupportGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DirectMessages",
                table: "DirectMessages");

            migrationBuilder.DropIndex(
                name: "IX_DirectMessages_ConversationId",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "SupportGroups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SupportGroups");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "CurrentMood",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentMoodIcon",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "DirectMessages",
                newName: "ReceiverId");

            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "DirectMessages",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessages_RecipientId",
                table: "DirectMessages",
                newName: "IX_DirectMessages_ReceiverId");

            migrationBuilder.RenameColumn(
                name: "SupportGroupGroupId",
                table: "ChatMessages",
                newName: "SupportGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SupportGroupGroupId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SupportGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SupportGroups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SupportGroups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DirectMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "SupportGroupId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportGroups",
                table: "SupportGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DirectMessages",
                table: "DirectMessages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SupportGroupId",
                table: "AspNetUsers",
                column: "SupportGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SupportGroups_SupportGroupId",
                table: "AspNetUsers",
                column: "SupportGroupId",
                principalTable: "SupportGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_SupportGroups_SupportGroupId",
                table: "ChatMessages",
                column: "SupportGroupId",
                principalTable: "SupportGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessages_AspNetUsers_ReceiverId",
                table: "DirectMessages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
