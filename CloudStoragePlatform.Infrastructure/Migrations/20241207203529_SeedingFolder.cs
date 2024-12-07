using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class SeedingFolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Folders_MetadataId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_SharingId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Files_MetadataId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_SharingId",
                table: "Files");

            migrationBuilder.AlterColumn<Guid>(
                name: "SharingId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MetadataId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SharingId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MetadataId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "FolderId", "CreationDate", "FolderName", "FolderParentId", "FolderPath", "IsFavorite", "IsTrash", "MetadataId", "RecentsRecentId", "SharingId" },
                values: new object[] { new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"), new DateTime(2024, 12, 8, 2, 5, 28, 960, DateTimeKind.Local).AddTicks(6606), "home", null, "C:\\CloudStoragePlatform\\home", false, false, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_MetadataId",
                table: "Folders",
                column: "MetadataId",
                unique: true,
                filter: "[MetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_SharingId",
                table: "Folders",
                column: "SharingId",
                unique: true,
                filter: "[SharingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Files_MetadataId",
                table: "Files",
                column: "MetadataId",
                unique: true,
                filter: "[MetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Files_SharingId",
                table: "Files",
                column: "SharingId",
                unique: true,
                filter: "[SharingId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Folders_MetadataId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_SharingId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Files_MetadataId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_SharingId",
                table: "Files");

            migrationBuilder.DeleteData(
                table: "Folders",
                keyColumn: "FolderId",
                keyValue: new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"));

            migrationBuilder.AlterColumn<Guid>(
                name: "SharingId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MetadataId",
                table: "Folders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SharingId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MetadataId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_MetadataId",
                table: "Folders",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_SharingId",
                table: "Folders",
                column: "SharingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_MetadataId",
                table: "Files",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_SharingId",
                table: "Files",
                column: "SharingId",
                unique: true);
        }
    }
}
