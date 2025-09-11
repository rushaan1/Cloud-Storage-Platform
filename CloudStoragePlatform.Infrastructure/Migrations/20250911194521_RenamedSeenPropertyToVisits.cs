using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    public partial class RenamedSeenPropertyToVisits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentShareLinkTimesSeen",
                table: "Shares",
                newName: "Visits");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Visits",
                table: "Shares",
                newName: "CurrentShareLinkTimesSeen");
        }
    }
}
