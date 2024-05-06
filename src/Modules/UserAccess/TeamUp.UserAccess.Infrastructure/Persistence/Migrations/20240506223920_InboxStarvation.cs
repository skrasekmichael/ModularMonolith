using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.UserAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InboxStarvation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailCount",
                schema: "UserAccess",
                table: "InboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextProcessingUtc",
                schema: "UserAccess",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailCount",
                schema: "UserAccess",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "NextProcessingUtc",
                schema: "UserAccess",
                table: "InboxMessages");
        }
    }
}
