using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yadeh.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialConversationLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: true),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationSnapshots_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationSnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessages", x => x.Id);
                    table.CheckConstraint("CK_ConversationMessages_Sequence", "\"Sequence\" >= 0");
                    table.ForeignKey(
                        name: "FK_ConversationMessages_ConversationSnapshots_ConversationSnapshotId",
                        column: x => x.ConversationSnapshotId,
                        principalTable: "ConversationSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ConversationSnapshotId_Sequence",
                table: "ConversationMessages",
                columns: new[] { "ConversationSnapshotId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatedAtUtc",
                table: "Conversations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationSnapshots_ConversationId_ImportedAtUtc",
                table: "ConversationSnapshots",
                columns: new[] { "ConversationId", "ImportedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationSnapshots_SourceUrl",
                table: "ConversationSnapshots",
                column: "SourceUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropTable(
                name: "ConversationSnapshots");

            migrationBuilder.DropTable(
                name: "Conversations");
        }
    }
}
