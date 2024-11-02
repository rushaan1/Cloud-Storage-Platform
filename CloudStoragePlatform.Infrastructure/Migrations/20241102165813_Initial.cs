using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Metadatasets",
                columns: table => new
                {
                    MetadataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousReplacementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplaceCount = table.Column<int>(type: "int", nullable: true),
                    PreviousRenameDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenameCount = table.Column<int>(type: "int", nullable: true),
                    PreviousPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousMoveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MoveCount = table.Column<int>(type: "int", nullable: true),
                    LastOpened = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenCount = table.Column<int>(type: "int", nullable: true),
                    ShareCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadatasets", x => x.MetadataId);
                });

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

            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    SharingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShareLinkUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareLinkExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentShareLinkTimesSeen = table.Column<int>(type: "int", nullable: true),
                    ShareLinkCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.SharingId);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolderParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FolderSize = table.Column<long>(type: "bigint", nullable: true),
                    RecentsRecentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SharingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetadataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsTrash = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.FolderId);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_FolderParentId",
                        column: x => x.FolderParentId,
                        principalTable: "Folders",
                        principalColumn: "FolderId");
                    table.ForeignKey(
                        name: "FK_Folders_Metadatasets_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatasets",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_Recents_RecentsRecentId",
                        column: x => x.RecentsRecentId,
                        principalTable: "Recents",
                        principalColumn: "RecentId");
                    table.ForeignKey(
                        name: "FK_Folders_Shares_SharingId",
                        column: x => x.SharingId,
                        principalTable: "Shares",
                        principalColumn: "SharingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecentsRecentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SharingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetadataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsTrash = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_Files_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "Folders",
                        principalColumn: "FolderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Files_Metadatasets_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatasets",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Recents_RecentsRecentId",
                        column: x => x.RecentsRecentId,
                        principalTable: "Recents",
                        principalColumn: "RecentId");
                    table.ForeignKey(
                        name: "FK_Files_Shares_SharingId",
                        column: x => x.SharingId,
                        principalTable: "Shares",
                        principalColumn: "SharingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_MetadataId",
                table: "Files",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentFolderId",
                table: "Files",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_RecentsRecentId",
                table: "Files",
                column: "RecentsRecentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_SharingId",
                table: "Files",
                column: "SharingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_FolderParentId",
                table: "Folders",
                column: "FolderParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_MetadataId",
                table: "Folders",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_RecentsRecentId",
                table: "Folders",
                column: "RecentsRecentId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_SharingId",
                table: "Folders",
                column: "SharingId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Metadatasets");

            migrationBuilder.DropTable(
                name: "Recents");

            migrationBuilder.DropTable(
                name: "Shares");
        }
    }
}
