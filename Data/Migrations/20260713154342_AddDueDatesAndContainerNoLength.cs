using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDatesAndContainerNoLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContainerNo",
                table: "JobIsnes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueLoa",
                table: "JobIsnes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueOrigin",
                table: "JobIsnes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueProformaInvoice",
                table: "JobIsnes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueLoa",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "DueOrigin",
                table: "JobIsnes");

            migrationBuilder.DropColumn(
                name: "DueProformaInvoice",
                table: "JobIsnes");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerNo",
                table: "JobIsnes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
