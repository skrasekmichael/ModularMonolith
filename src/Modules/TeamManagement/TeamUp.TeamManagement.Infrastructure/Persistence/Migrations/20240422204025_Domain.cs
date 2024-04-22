using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NumberOfMembers = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventType",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventType_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "TeamManagement",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "TeamManagement",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalSchema: "TeamManagement",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMember",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMember_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "TeamManagement",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMember_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "TeamManagement",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MeetTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ReplyClosingTimeBeforeMeetTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_EventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalSchema: "TeamManagement",
                        principalTable: "EventType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "TeamManagement",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventResponse",
                schema: "TeamManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplyType = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventResponse_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "TeamManagement",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventResponse_TeamMember_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalSchema: "TeamManagement",
                        principalTable: "TeamMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventResponse_EventId",
                schema: "TeamManagement",
                table: "EventResponse",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventResponse_EventId_TeamMemberId",
                schema: "TeamManagement",
                table: "EventResponse",
                columns: new[] { "EventId", "TeamMemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventResponse_TeamMemberId",
                schema: "TeamManagement",
                table: "EventResponse",
                column: "TeamMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                schema: "TeamManagement",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TeamId",
                schema: "TeamManagement",
                table: "Events",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EventType_TeamId",
                schema: "TeamManagement",
                table: "EventType",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_RecipientId",
                schema: "TeamManagement",
                table: "Invitations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TeamId",
                schema: "TeamManagement",
                table: "Invitations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TeamId_RecipientId",
                schema: "TeamManagement",
                table: "Invitations",
                columns: new[] { "TeamId", "RecipientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_TeamId",
                schema: "TeamManagement",
                table: "TeamMember",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_UserId",
                schema: "TeamManagement",
                table: "TeamMember",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventResponse",
                schema: "TeamManagement");

            migrationBuilder.DropTable(
                name: "Invitations",
                schema: "TeamManagement");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "TeamManagement");

            migrationBuilder.DropTable(
                name: "TeamMember",
                schema: "TeamManagement");

            migrationBuilder.DropTable(
                name: "EventType",
                schema: "TeamManagement");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "TeamManagement");
        }
    }
}
