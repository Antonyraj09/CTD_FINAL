using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyBranchesAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: the auto-scaffolded version of this migration mis-detected City/Phone/Email/Gstin
            // as renamed into the new ContactPerson*/AdCode columns (a column-position heuristic, not a
            // real rename) and dropped City outright. This hand-written version keeps every new column
            // genuinely new/nullable and copies each party's existing City/Phone/Email/Gstin into a real
            // "Head Office" branch record before dropping those columns from Parties.
            migrationBuilder.DropIndex(
                name: "IX_Parties_Gstin",
                table: "Parties");

            migrationBuilder.AddColumn<string>(name: "TradeName", table: "Parties", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<int>(name: "Constitution", table: "Parties", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "Pan", table: "Parties", type: "nvarchar(10)", maxLength: 10, nullable: true);
            migrationBuilder.AddColumn<string>(name: "IecCode", table: "Parties", type: "nvarchar(15)", maxLength: 15, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CinNumber", table: "Parties", type: "nvarchar(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "LicenseValidUpto", table: "Parties", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<int>(name: "AeoStatus", table: "Parties", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "AeoCertificateNo", table: "Parties", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "BankName", table: "Parties", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "BankAccountNo", table: "Parties", type: "nvarchar(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "BankIfsc", table: "Parties", type: "nvarchar(15)", maxLength: 15, nullable: true);
            migrationBuilder.AddColumn<string>(name: "AdCode", table: "Parties", type: "nvarchar(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Website", table: "Parties", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ContactPersonName", table: "Parties", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ContactPersonDesignation", table: "Parties", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ContactPersonPhone", table: "Parties", type: "nvarchar(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ContactPersonEmail", table: "Parties", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsActive", table: "Parties", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<string>(name: "Remarks", table: "Parties", type: "nvarchar(1000)", maxLength: 1000, nullable: true);

            migrationBuilder.CreateTable(
                name: "PartyBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Gstin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CustomsRegistrationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyBranches_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Every existing Party becomes its own "Head Office" branch, carrying forward its
            // City/Phone/Email/Gstin — no address line existed before this migration, so City doubles
            // as a placeholder AddressLine1 (matches this build's own seed-data convention).
            migrationBuilder.Sql(@"
INSERT INTO [PartyBranches] ([PartyId],[BranchName],[IsPrimary],[IsActive],[AddressLine1],[City],[Country],[Gstin],[Phone],[Email],[CreatedAt],[UpdatedAt])
SELECT [Id], 'Head Office', 1, 1, [City], [City], 'India', [Gstin], [Phone], [Email], [CreatedAt], [UpdatedAt]
FROM [Parties];");

            migrationBuilder.DropColumn(name: "City", table: "Parties");
            migrationBuilder.DropColumn(name: "Phone", table: "Parties");
            migrationBuilder.DropColumn(name: "Email", table: "Parties");
            migrationBuilder.DropColumn(name: "Gstin", table: "Parties");

            migrationBuilder.CreateIndex(name: "IX_Parties_CinNumber", table: "Parties", column: "CinNumber", unique: true, filter: "[CinNumber] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_Parties_IecCode", table: "Parties", column: "IecCode", unique: true, filter: "[IecCode] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_Parties_Pan", table: "Parties", column: "Pan", unique: true, filter: "[Pan] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_PartyBranches_Gstin", table: "PartyBranches", column: "Gstin", unique: true, filter: "[Gstin] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_PartyBranches_PartyId", table: "PartyBranches", column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restores City/Phone/Email/Gstin from each party's primary branch (best-effort — if a
            // party has since been edited to have more than one branch, or no primary branch, this
            // cannot fully reconstruct the original single-address shape).
            migrationBuilder.AddColumn<string>(name: "City", table: "Parties", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Phone", table: "Parties", type: "nvarchar(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Email", table: "Parties", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Gstin", table: "Parties", type: "nvarchar(20)", maxLength: 20, nullable: true);

            migrationBuilder.Sql(@"
UPDATE p SET p.[City] = b.[City], p.[Phone] = b.[Phone], p.[Email] = b.[Email], p.[Gstin] = b.[Gstin]
FROM [Parties] p
JOIN [PartyBranches] b ON b.[PartyId] = p.[Id] AND b.[IsPrimary] = 1;");

            migrationBuilder.DropTable(name: "PartyBranches");

            migrationBuilder.DropIndex(name: "IX_Parties_CinNumber", table: "Parties");
            migrationBuilder.DropIndex(name: "IX_Parties_IecCode", table: "Parties");
            migrationBuilder.DropIndex(name: "IX_Parties_Pan", table: "Parties");

            migrationBuilder.DropColumn(name: "AeoCertificateNo", table: "Parties");
            migrationBuilder.DropColumn(name: "AeoStatus", table: "Parties");
            migrationBuilder.DropColumn(name: "BankAccountNo", table: "Parties");
            migrationBuilder.DropColumn(name: "BankIfsc", table: "Parties");
            migrationBuilder.DropColumn(name: "BankName", table: "Parties");
            migrationBuilder.DropColumn(name: "CinNumber", table: "Parties");
            migrationBuilder.DropColumn(name: "Constitution", table: "Parties");
            migrationBuilder.DropColumn(name: "ContactPersonDesignation", table: "Parties");
            migrationBuilder.DropColumn(name: "ContactPersonEmail", table: "Parties");
            migrationBuilder.DropColumn(name: "ContactPersonName", table: "Parties");
            migrationBuilder.DropColumn(name: "ContactPersonPhone", table: "Parties");
            migrationBuilder.DropColumn(name: "AdCode", table: "Parties");
            migrationBuilder.DropColumn(name: "IecCode", table: "Parties");
            migrationBuilder.DropColumn(name: "IsActive", table: "Parties");
            migrationBuilder.DropColumn(name: "LicenseValidUpto", table: "Parties");
            migrationBuilder.DropColumn(name: "Pan", table: "Parties");
            migrationBuilder.DropColumn(name: "Remarks", table: "Parties");
            migrationBuilder.DropColumn(name: "TradeName", table: "Parties");
            migrationBuilder.DropColumn(name: "Website", table: "Parties");

            migrationBuilder.CreateIndex(name: "IX_Parties_Gstin", table: "Parties", column: "Gstin", unique: true, filter: "[Gstin] IS NOT NULL");
        }
    }
}
