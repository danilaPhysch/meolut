using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EphemerisHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeidouTle",
                columns: table => new
                {
                    CsSatNum = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Line1 = table.Column<string>(type: "text", nullable: false),
                    Line2 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeidouTle", x => new { x.CsSatNum, x.Time });
                });

            migrationBuilder.CreateTable(
                name: "GalileoTle",
                columns: table => new
                {
                    CsSatNum = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Line1 = table.Column<string>(type: "text", nullable: false),
                    Line2 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalileoTle", x => new { x.CsSatNum, x.Time });
                });

            migrationBuilder.CreateTable(
                name: "GlonassTle",
                columns: table => new
                {
                    CsSatNum = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Line1 = table.Column<string>(type: "text", nullable: false),
                    Line2 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlonassTle", x => new { x.CsSatNum, x.Time });
                });

            migrationBuilder.CreateTable(
                name: "GpsTle",
                columns: table => new
                {
                    CsSatNum = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Line1 = table.Column<string>(type: "text", nullable: false),
                    Line2 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsTle", x => new { x.CsSatNum, x.Time });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeidouTle");

            migrationBuilder.DropTable(
                name: "GalileoTle");

            migrationBuilder.DropTable(
                name: "GlonassTle");

            migrationBuilder.DropTable(
                name: "GpsTle");
        }
    }
}
