using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class CombineJobIsneContainerGrid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContainerNo",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "ContainerSize",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CustomsCode",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "MarksSerial",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "NoPackages",
                table: "JobIsnes");

            // Old single-container ContainerStatus (FCL/LCL) becomes the new
            // job-level ShipmentType default — renamed, not dropped, so
            // existing FCL/LCL values on pre-existing rows are preserved.
            migrationBuilder.RenameColumn(
                name: "ContainerStatus",
                table: "JobIsnes",
                newName: "ShipmentType");

            migrationBuilder.CreateTable(
                name: "JobIsneContainers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobIsneId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ContainerNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ContainerSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipmentType = table.Column<int>(type: "int", nullable: false),
                    NoPackages = table.Column<int>(type: "int", nullable: false),
                    PackageType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    GrossWeightUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    NetWeightUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MarksSerial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomsCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobIsneContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobIsneContainers_JobIsnes_JobIsneId",
                        column: x => x.JobIsneId,
                        principalTable: "JobIsnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobIsneContainers_ContainerNo",
                table: "JobIsneContainers",
                column: "ContainerNo");

            migrationBuilder.CreateIndex(
                name: "IX_JobIsneContainers_JobIsneId",
                table: "JobIsneContainers",
                column: "JobIsneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobIsneContainers");

            migrationBuilder.RenameColumn(
                name: "ShipmentType",
                table: "JobIsnes",
                newName: "ContainerStatus");

            migrationBuilder.AddColumn<int>(
                name: "NoPackages",
                table: "JobIsnes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContainerNo",
                table: "JobIsnes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContainerSize",
                table: "JobIsnes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomsCode",
                table: "JobIsnes",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarksSerial",
                table: "JobIsnes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "JobIsnes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
