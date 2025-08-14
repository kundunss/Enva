using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SystemHardware",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Sites",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemHardware_CompanyId",
                table: "SystemHardware",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CompanyId",
                table: "Sites",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Companies_CompanyId",
                table: "Sites",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemHardware_Companies_CompanyId",
                table: "SystemHardware",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Companies_CompanyId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemHardware_Companies_CompanyId",
                table: "SystemHardware");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_SystemHardware_CompanyId",
                table: "SystemHardware");

            migrationBuilder.DropIndex(
                name: "IX_Sites_CompanyId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SystemHardware");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Sites");
        }
    }
}
