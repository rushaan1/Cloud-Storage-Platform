using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class RenamedParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_FolderParentId",
                table: "Folders");

            migrationBuilder.RenameColumn(
                name: "FolderParentId",
                table: "Folders",
                newName: "ParentFolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_FolderParentId",
                table: "Folders",
                newName: "IX_Folders_ParentFolderId");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "FolderId",
                keyValue: new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"),
                column: "CreationDate",
                value: new DateTime(2024, 12, 8, 9, 19, 52, 262, DateTimeKind.Local).AddTicks(9408));

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId",
                principalTable: "Folders",
                principalColumn: "FolderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_ParentFolderId",
                table: "Folders");

            migrationBuilder.RenameColumn(
                name: "ParentFolderId",
                table: "Folders",
                newName: "FolderParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                newName: "IX_Folders_FolderParentId");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "FolderId",
                keyValue: new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"),
                column: "CreationDate",
                value: new DateTime(2024, 12, 8, 2, 5, 28, 960, DateTimeKind.Local).AddTicks(6606));

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_FolderParentId",
                table: "Folders",
                column: "FolderParentId",
                principalTable: "Folders",
                principalColumn: "FolderId");
        }
    }
}
