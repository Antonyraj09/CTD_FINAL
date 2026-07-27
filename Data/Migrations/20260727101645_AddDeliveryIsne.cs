using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryIsne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryIsnes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNo = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartYN = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    JobIsneId = table.Column<int>(type: "int", nullable: false),
                    JobNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConsigneeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TruckRailwayReckNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Shed = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    KeyNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Package = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TransporterId = table.Column<int>(type: "int", nullable: true),
                    TransporterCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TransporterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BslNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    StaffCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StaffName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContainerNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ContainerSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryIsnes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryIsnes_JobIsnes_JobIsneId",
                        column: x => x.JobIsneId,
                        principalTable: "JobIsnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryIsnes_Deleted",
                table: "DeliveryIsnes",
                column: "Deleted");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryIsnes_DeliveryDate",
                table: "DeliveryIsnes",
                column: "DeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryIsnes_JobIsneId",
                table: "DeliveryIsnes",
                column: "JobIsneId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryIsnes_JobNo",
                table: "DeliveryIsnes",
                column: "JobNo");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryIsnes_SerialNo",
                table: "DeliveryIsnes",
                column: "SerialNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryIsnes");
        }
    }
}
