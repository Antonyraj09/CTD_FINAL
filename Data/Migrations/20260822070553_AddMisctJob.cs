using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMisctJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MisctJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    JobDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartyCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubAgentCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SubAgentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VesselName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    VoyageNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CountryCgn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RotNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RotDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LineNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    MblNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    MblDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomsStationExitId = table.Column<int>(type: "int", nullable: true),
                    CustomsStationExitName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PortOfEntryNepalId = table.Column<int>(type: "int", nullable: true),
                    PortOfEntryNepalName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Container20Qty = table.Column<int>(type: "int", nullable: false),
                    Container40Qty = table.Column<int>(type: "int", nullable: false),
                    LclQty = table.Column<int>(type: "int", nullable: false),
                    CustomCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NoOfPackage = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CarrierAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CarrierGstin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    QuantityUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisctJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisctJobs_BorderPoints_PortOfEntryNepalId",
                        column: x => x.PortOfEntryNepalId,
                        principalTable: "BorderPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MisctJobs_CustomsHouses_CustomsStationExitId",
                        column: x => x.CustomsStationExitId,
                        principalTable: "CustomsHouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MisctJobContainers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MisctJobId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ContainerNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SealNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContainerSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    CifValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisctJobContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisctJobContainers_MisctJobs_MisctJobId",
                        column: x => x.MisctJobId,
                        principalTable: "MisctJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MisctJobContainers_ContainerNo",
                table: "MisctJobContainers",
                column: "ContainerNo");

            migrationBuilder.CreateIndex(
                name: "IX_MisctJobContainers_MisctJobId",
                table: "MisctJobContainers",
                column: "MisctJobId");

            migrationBuilder.CreateIndex(
                name: "IX_MisctJobs_CustomsStationExitId",
                table: "MisctJobs",
                column: "CustomsStationExitId");

            migrationBuilder.CreateIndex(
                name: "IX_MisctJobs_JobNo",
                table: "MisctJobs",
                column: "JobNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MisctJobs_PortOfEntryNepalId",
                table: "MisctJobs",
                column: "PortOfEntryNepalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MisctJobContainers");

            migrationBuilder.DropTable(
                name: "MisctJobs");
        }
    }
}
