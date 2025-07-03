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
                name: "BeidouEphemeris",
                columns: table => new
                {
                    SatellitePrn = table.Column<int>(type: "integer", nullable: false),
                    TimeOfClock = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aode = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crs = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    DeltaN = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    M0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cuc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Eccentricity = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cus = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SqrtA = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Toe = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cic = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cis = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    I0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    OmegaDot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Idot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Spare1 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    BdtWeek = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Spare2 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SvAccuracy = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SatH1 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Tgd1 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Tgd2 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    TransmissionTime = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Aodc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SatelliteSystem = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ClockBias = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDrift = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDriftRate = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeidouEphemeris", x => new { x.SatellitePrn, x.TimeOfClock });
                });

            migrationBuilder.CreateTable(
                name: "GalileoEphemeris",
                columns: table => new
                {
                    SatellitePrn = table.Column<int>(type: "integer", nullable: false),
                    TimeOfClock = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Iodnav = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crs = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    DeltaN = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    M0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cuc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Eccentricity = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cus = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SqrtA = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Toe = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cic = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cis = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    I0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    OmegaDot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Idot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    DataSources = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    GalWeek = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Spare = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Sisa = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SvHealth = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    BgdE5aE1 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    BgdE5bE1 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    TransmissionTime = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SatelliteSystem = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ClockBias = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDrift = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDriftRate = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalileoEphemeris", x => new { x.SatellitePrn, x.TimeOfClock });
                });

            migrationBuilder.CreateTable(
                name: "GlonassEphemeris",
                columns: table => new
                {
                    SatellitePrn = table.Column<int>(type: "integer", nullable: false),
                    TimeOfClock = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PosX = table.Column<double>(type: "double precision", precision: 15, scale: 3, nullable: false),
                    VelX = table.Column<double>(type: "double precision", precision: 15, scale: 9, nullable: false),
                    AccX = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Health = table.Column<double>(type: "double precision", nullable: false),
                    PosY = table.Column<double>(type: "double precision", precision: 15, scale: 3, nullable: false),
                    VelY = table.Column<double>(type: "double precision", precision: 15, scale: 9, nullable: false),
                    AccY = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    FreqNum = table.Column<double>(type: "double precision", nullable: false),
                    PosZ = table.Column<double>(type: "double precision", precision: 15, scale: 3, nullable: false),
                    VelZ = table.Column<double>(type: "double precision", precision: 15, scale: 9, nullable: false),
                    AccZ = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    AgeOfOperation = table.Column<double>(type: "double precision", nullable: false),
                    SatelliteSystem = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ClockBias = table.Column<double>(type: "double precision", nullable: false),
                    ClockDrift = table.Column<double>(type: "double precision", nullable: false),
                    ClockDriftRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlonassEphemeris", x => new { x.SatellitePrn, x.TimeOfClock });
                });

            migrationBuilder.CreateTable(
                name: "GpsEphemeris",
                columns: table => new
                {
                    SatellitePrn = table.Column<int>(type: "integer", nullable: false),
                    TimeOfClock = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Iode = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crs = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    DeltaN = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    M0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cuc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Eccentricity = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cus = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    SqrtA = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Toe = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cic = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Cis = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    I0 = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Crc = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Omega = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    OmegaDot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    Idot = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    CodesOnL2 = table.Column<double>(type: "double precision", nullable: false),
                    GpsWeek = table.Column<double>(type: "double precision", nullable: false),
                    L2PDataFlag = table.Column<double>(type: "double precision", nullable: false),
                    SvAccuracy = table.Column<double>(type: "double precision", nullable: false),
                    SvHealth = table.Column<double>(type: "double precision", nullable: false),
                    Tgd = table.Column<double>(type: "double precision", nullable: false),
                    Iodc = table.Column<double>(type: "double precision", nullable: false),
                    TransmissionTime = table.Column<double>(type: "double precision", nullable: false),
                    FitInterval = table.Column<double>(type: "double precision", nullable: false),
                    SatelliteSystem = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ClockBias = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDrift = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false),
                    ClockDriftRate = table.Column<double>(type: "double precision", precision: 15, scale: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsEphemeris", x => new { x.SatellitePrn, x.TimeOfClock });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeidouEphemeris");

            migrationBuilder.DropTable(
                name: "GalileoEphemeris");

            migrationBuilder.DropTable(
                name: "GlonassEphemeris");

            migrationBuilder.DropTable(
                name: "GpsEphemeris");
        }
    }
}
