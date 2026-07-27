using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CTD_FINAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillDeliveryIsnePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TenantSeeder only seeds RolePermissions when the table is empty, so a brand-new
            // PermissionKeys entry never reaches an already-seeded tenant database on its own.
            // Guard on JobIsne.Manage already existing (proof this tenant was seeded before this
            // permission was introduced) and DeliveryIsne.Manage not existing yet, so this is a
            // no-op both for genuinely fresh tenants (TenantSeeder will insert the full matrix,
            // including this key, right after migrations run) and for databases that already have it.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM RolePermissions WHERE ModuleKey = 'JobIsne.Manage')
   AND NOT EXISTS (SELECT 1 FROM RolePermissions WHERE ModuleKey = 'DeliveryIsne.Manage')
BEGIN
    INSERT INTO RolePermissions (Role, ModuleKey, Allowed, CreatedAt) VALUES
        (N'Administrator', N'DeliveryIsne.Manage', 1, SYSUTCDATETIME()),
        (N'Manager', N'DeliveryIsne.Manage', 1, SYSUTCDATETIME()),
        (N'Operator', N'DeliveryIsne.Manage', 0, SYSUTCDATETIME()),
        (N'Viewer', N'DeliveryIsne.Manage', 0, SYSUTCDATETIME());
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM RolePermissions WHERE ModuleKey = 'DeliveryIsne.Manage';");
        }
    }
}
