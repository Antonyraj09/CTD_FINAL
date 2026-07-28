using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations.Admin
{
    /// <inheritdoc />
    public partial class AddInstallationHistoryRequestSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "InstallationHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminEmail",
                table: "InstallationHistories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminFullName",
                table: "InstallationHistories",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "InstallationHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "InstallationHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "InstallationHistories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "InstallationHistories",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "InstallationHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseName",
                table: "InstallationHistories",
                type: "nvarchar(63)",
                maxLength: 63,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseUsername",
                table: "InstallationHistories",
                type: "nvarchar(63)",
                maxLength: 63,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "InstallationHistories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstNumber",
                table: "InstallationHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallationLocation",
                table: "InstallationHistories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseType",
                table: "InstallationHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "InstallationHistories",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "InstallationHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "AdminEmail",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "AdminFullName",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "City",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "DatabaseName",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "DatabaseUsername",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "GstNumber",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "InstallationLocation",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "InstallationHistories");

            migrationBuilder.DropColumn(
                name: "State",
                table: "InstallationHistories");
        }
    }
}
