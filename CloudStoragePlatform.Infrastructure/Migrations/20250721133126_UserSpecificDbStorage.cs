using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class UserSpecificDbStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Shares",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Metadatasets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Shares_UserId",
                table: "Shares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Metadatasets_UserId",
                table: "Metadatasets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId",
                table: "Folders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UserId",
                table: "Files",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_AspNetUsers_UserId",
                table: "Files",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_UserId",
                table: "Folders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Metadatasets_AspNetUsers_UserId",
                table: "Metadatasets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shares_AspNetUsers_UserId",
                table: "Shares",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_AspNetUsers_UserId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_UserId",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_Metadatasets_AspNetUsers_UserId",
                table: "Metadatasets");

            migrationBuilder.DropForeignKey(
                name: "FK_Shares_AspNetUsers_UserId",
                table: "Shares");

            migrationBuilder.DropIndex(
                name: "IX_Shares_UserId",
                table: "Shares");

            migrationBuilder.DropIndex(
                name: "IX_Metadatasets_UserId",
                table: "Metadatasets");

            migrationBuilder.DropIndex(
                name: "IX_Folders_UserId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Files_UserId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Metadatasets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Files");

            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "FolderId", "CreationDate", "FolderName", "FolderPath", "IsFavorite", "IsTrash", "MetadataId", "ParentFolderId", "SharingId", "Size" },
                values: new object[] { new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"), new DateTime(2025, 7, 15, 15, 15, 34, 660, DateTimeKind.Local).AddTicks(9829), "home", "C:\\CloudStoragePlatform\\home", false, false, null, null, null, 0f });
        }
    }
}
