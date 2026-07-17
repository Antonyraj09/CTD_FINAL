using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryForDataSheetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateOfOrigin",
                table: "JobIsnes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CertificateOfOriginDate",
                table: "JobIsnes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImporterCode",
                table: "JobIsnes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCompanyNameAddress",
                table: "JobIsnes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate",
                table: "JobIsnes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "JobIsnes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SensitiveCargo",
                table: "JobIsnes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SensitiveCifValue",
                table: "JobIsnes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateOfOrigin",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "CertificateOfOriginDate",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "ImporterCode",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "InsuranceCompanyNameAddress",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "SensitiveCargo",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "SensitiveCifValue",
                table: "JobIsnes");
        }
    }
}
