using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShrinkImporterCodeToSixChars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImporterCode",
                table: "JobIsnes",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImporterCode",
                table: "JobIsnes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);
        }
    }
}
