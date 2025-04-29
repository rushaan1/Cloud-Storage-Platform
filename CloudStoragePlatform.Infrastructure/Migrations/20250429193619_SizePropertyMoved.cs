using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class SizePropertyMoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Metadatasets");

            migrationBuilder.AddColumn<float>(
                name: "Size",
                table: "Folders",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Size",
                table: "Files",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Files");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "Metadatasets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

        }
    }
}
