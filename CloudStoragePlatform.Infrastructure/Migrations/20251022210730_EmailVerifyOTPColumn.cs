using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class EmailVerifyOTPColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationOTP",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOTPExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationOTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOTPExpiresAt",
                table: "AspNetUsers");
        }
    }
}
