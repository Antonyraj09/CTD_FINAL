using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillMisctPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM RolePermissions WHERE ModuleKey = 'JobIsne.Manage')
            AND NOT EXISTS (SELECT 1 FROM RolePermissions WHERE ModuleKey = 'Misct.Manage')
            BEGIN
            INSERT INTO RolePermissions (Role, ModuleKey, Allowed, CreatedAt) VALUES
            (N'Administrator', N'Misct.Manage', 1, SYSUTCDATETIME()),
            (N'Manager', N'Misct.Manage', 1, SYSUTCDATETIME()),
            (N'Operator', N'Misct.Manage', 0, SYSUTCDATETIME()),
            (N'Viewer', N'Misct.Manage', 0, SYSUTCDATETIME());
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM RolePermissions WHERE ModuleKey = 'Misct.Manage';");
        }
    }
}
