using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableCompanyIdToDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Domains",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Domains_CompanyId",
                table: "Domains",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Domains_Companies_CompanyId",
                table: "Domains",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Domains_Companies_CompanyId",
                table: "Domains");

            migrationBuilder.DropIndex(
                name: "IX_Domains_CompanyId",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Domains");
        }
    }
}
