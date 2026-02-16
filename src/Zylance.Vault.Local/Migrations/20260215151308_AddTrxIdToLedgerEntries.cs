using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zylance.Vault.Local.Migrations
{
    /// <inheritdoc />
    public partial class AddTrxIdToLedgerEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrxId",
                table: "LedgerEntries",
                type: "TEXT",
                maxLength: 255,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TrxId", table: "LedgerEntries");
        }
    }
}
