using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSCoffee.Common.Migrations
{
    /// <inheritdoc />
    public partial class RoomsAndPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "room_memberships",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_memberships_players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_room_memberships_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_players_Token",
                schema: "public",
                table: "players",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_memberships_PlayerId",
                schema: "public",
                table: "room_memberships",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_room_memberships_RoomId_PlayerId",
                schema: "public",
                table: "room_memberships",
                columns: new[] { "RoomId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_Code",
                schema: "public",
                table: "rooms",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room_memberships",
                schema: "public");

            migrationBuilder.DropTable(
                name: "players",
                schema: "public");

            migrationBuilder.DropTable(
                name: "rooms",
                schema: "public");
        }
    }
}
