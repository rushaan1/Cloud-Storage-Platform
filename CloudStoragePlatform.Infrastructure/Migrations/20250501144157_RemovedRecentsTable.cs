using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class RemovedRecentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Recents_RecentsRecentId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Recents_RecentsRecentId",
                table: "Folders");

            migrationBuilder.DropTable(
                name: "Recents");

            migrationBuilder.DropIndex(
                name: "IX_Folders_RecentsRecentId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Files_RecentsRecentId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "RecentsRecentId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "RecentsRecentId",
                table: "Files");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecentsRecentId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecentsRecentId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Recents",
                columns: table => new
                {
                    RecentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recents", x => x.RecentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_RecentsRecentId",
                table: "Folders",
                column: "RecentsRecentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_RecentsRecentId",
                table: "Files",
                column: "RecentsRecentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Recents_RecentsRecentId",
                table: "Files",
                column: "RecentsRecentId",
                principalTable: "Recents",
                principalColumn: "RecentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Recents_RecentsRecentId",
                table: "Folders",
                column: "RecentsRecentId",
                principalTable: "Recents",
                principalColumn: "RecentId");
        }
    }
}
