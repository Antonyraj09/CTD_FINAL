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
            // Any existing value was written under the old "NP0000" + 4-digit (10-char)
            // format, which doesn't fit the new 6-char column and doesn't carry over
            // meaningfully anyway (the old fixed "NP0000" prefix has no equivalent in
            // the new 2-letter + 4-digit shape). Clear out-of-range values first rather
            // than let SQL Server hard-fail the ALTER COLUMN with a truncation error.
            migrationBuilder.Sql("UPDATE [JobIsnes] SET [ImporterCode] = NULL WHERE [ImporterCode] IS NOT NULL AND LEN([ImporterCode]) > 6;");

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
