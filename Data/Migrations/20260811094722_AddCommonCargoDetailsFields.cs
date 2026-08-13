using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommonCargoDetailsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CargoGrossWeight",
                table: "JobIsnes",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoGrossWeightUnit",
                table: "JobIsnes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CargoNetWeight",
                table: "JobIsnes",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoNetWeightUnit",
                table: "JobIsnes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CargoPackageUnit",
                table: "JobIsnes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CargoPackages",
                table: "JobIsnes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoGrossWeight",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CargoGrossWeightUnit",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CargoNetWeight",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CargoNetWeightUnit",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CargoPackageUnit",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CargoPackages",
                table: "JobIsnes");
        }
    }
}
