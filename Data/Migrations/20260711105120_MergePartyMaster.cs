using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergePartyMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. New unified table, still empty.
            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsImporter = table.Column<bool>(type: "bit", nullable: false),
                    IsTransporter = table.Column<bool>(type: "bit", nullable: false),
                    IsAgent = table.Column<bool>(type: "bit", nullable: false),
                    Gstin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    License = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fleet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            // 2. Drop the old FKs before we start rewriting CtdJobs.ImporterId/AgentId/TransporterId —
            // those columns still reference Importers/Agents/Transporters until step 4, and SQL Server
            // would reject an UPDATE that momentarily points them at a Parties.Id under the old constraint.
            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Agents_AgentId",
                table: "CtdJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Importers_ImporterId",
                table: "CtdJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Transporters_TransporterId",
                table: "CtdJobs");

            // 3-4. Copy every existing Importer/Agent/Transporter row into Parties (tagged with its
            // original single role), then remap CtdJobs' existing FK values from the old per-table IDs
            // to the newly assigned Parties IDs, so no existing job loses its importer/agent/transporter.
            // SQL Server's OUTPUT INTO clause cannot reference the source table of an INSERT...SELECT
            // (only the inserted/deleted pseudo-tables), so the old ID is carried across as a normal
            // staging column instead, dropped once the CtdJobs remap is done. Each step is its own
            // Sql()/migrationBuilder call (= its own batch) because a column added by ALTER TABLE isn't
            // visible to later statements in the same batch under SQL Server's deferred name resolution.
            migrationBuilder.AddColumn<int>(name: "LegacyImporterId", table: "Parties", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "LegacyAgentId", table: "Parties", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "LegacyTransporterId", table: "Parties", type: "int", nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyImporterId])
SELECT [Name],[City],[Phone],[Email],1,0,0,[Gstin],NULL,NULL,[CreatedAt],[UpdatedAt],[Id]
FROM [Importers];");

            migrationBuilder.Sql(@"
INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyAgentId])
SELECT [Name],[City],[Phone],[Email],0,0,1,NULL,[License],NULL,[CreatedAt],[UpdatedAt],[Id]
FROM [Agents];");

            migrationBuilder.Sql(@"
INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyTransporterId])
SELECT [Name],[City],[Phone],[Email],0,1,0,NULL,NULL,[Fleet],[CreatedAt],[UpdatedAt],[Id]
FROM [Transporters];");

            migrationBuilder.Sql(@"
UPDATE j SET j.[ImporterId] = p.[Id]
FROM [CtdJobs] j
JOIN [Parties] p ON p.[LegacyImporterId] = j.[ImporterId];");

            migrationBuilder.Sql(@"
UPDATE j SET j.[AgentId] = p.[Id]
FROM [CtdJobs] j
JOIN [Parties] p ON p.[LegacyAgentId] = j.[AgentId];");

            migrationBuilder.Sql(@"
UPDATE j SET j.[TransporterId] = p.[Id]
FROM [CtdJobs] j
JOIN [Parties] p ON p.[LegacyTransporterId] = j.[TransporterId];");

            migrationBuilder.DropColumn(name: "LegacyImporterId", table: "Parties");
            migrationBuilder.DropColumn(name: "LegacyAgentId", table: "Parties");
            migrationBuilder.DropColumn(name: "LegacyTransporterId", table: "Parties");

            // 5. Old per-role tables are now fully superseded.
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Importers");

            migrationBuilder.DropTable(
                name: "Transporters");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Gstin",
                table: "Parties",
                column: "Gstin",
                unique: true,
                filter: "[Gstin] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_License",
                table: "Parties",
                column: "License",
                unique: true,
                filter: "[License] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Name",
                table: "Parties",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Parties_AgentId",
                table: "CtdJobs",
                column: "AgentId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Parties_ImporterId",
                table: "CtdJobs",
                column: "ImporterId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Parties_TransporterId",
                table: "CtdJobs",
                column: "TransporterId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NOTE: this restores the old table shapes but does not reverse-migrate data. Once a Party
            // has been tagged with more than one role it can no longer be split back into single-role
            // Importer/Agent/Transporter rows unambiguously, so rolling back after any multi-role
            // record has been created (or edited) will leave Importers/Agents/Transporters empty.
            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Parties_AgentId",
                table: "CtdJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Parties_ImporterId",
                table: "CtdJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_CtdJobs_Parties_TransporterId",
                table: "CtdJobs");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    License = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Importers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Gstin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Importers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transporters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Fleet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transporters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_License",
                table: "Agents",
                column: "License",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Name",
                table: "Agents",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Importers_Gstin",
                table: "Importers",
                column: "Gstin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Importers_Name",
                table: "Importers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Transporters_Name",
                table: "Transporters",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Agents_AgentId",
                table: "CtdJobs",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Importers_ImporterId",
                table: "CtdJobs",
                column: "ImporterId",
                principalTable: "Importers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CtdJobs_Transporters_TransporterId",
                table: "CtdJobs",
                column: "TransporterId",
                principalTable: "Transporters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
