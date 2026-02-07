using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zylance.Vault.Local.Entities
{
    /// <inheritdoc />
    public partial class AddZylanceMarkerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_zylance_",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__zylance_", x => x.Key);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "_zylance_");
        }
    }
}
