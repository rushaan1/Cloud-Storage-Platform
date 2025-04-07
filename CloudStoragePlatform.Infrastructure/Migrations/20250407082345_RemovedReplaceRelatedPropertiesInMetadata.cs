using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class RemovedReplaceRelatedPropertiesInMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousReplacementDate",
                table: "Metadatasets");

            migrationBuilder.DropColumn(
                name: "ReplaceCount",
                table: "Metadatasets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousReplacementDate",
                table: "Metadatasets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReplaceCount",
                table: "Metadatasets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
