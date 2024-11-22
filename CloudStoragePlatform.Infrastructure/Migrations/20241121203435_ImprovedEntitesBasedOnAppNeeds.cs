using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class ImprovedEntitesBasedOnAppNeeds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderSize",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Files");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "Metadatasets",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Metadatasets");

            migrationBuilder.AddColumn<long>(
                name: "FolderSize",
                table: "Folders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Files",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
