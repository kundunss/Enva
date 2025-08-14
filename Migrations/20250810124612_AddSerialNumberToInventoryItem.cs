using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialNumberToInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First add the column as nullable
            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "InventoryItems",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            // Update existing records with unique serial numbers
            migrationBuilder.Sql(@"
                UPDATE InventoryItems 
                SET SerialNumber = 'SN' || printf('%06d', Id)
                WHERE SerialNumber IS NULL;
            ");

            // Now make the column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "InventoryItems",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_SerialNumber",
                table: "InventoryItems",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_SerialNumber",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "InventoryItems");
        }
    }
}
