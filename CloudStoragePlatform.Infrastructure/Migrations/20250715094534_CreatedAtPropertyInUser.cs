using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class CreatedAtPropertyInUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "FolderId",
                keyValue: new Guid("9e2abd0a-94ac-43e2-a212-9dc9f7590447"),
                column: "CreationDate",
                value: new DateTime(2025, 7, 9, 17, 21, 9, 645, DateTimeKind.Local).AddTicks(8629));
        }
    }
}
