using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AylikBorcVeOdemeSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaksitSayisi",
                table: "OkulUcretleri",
                newName: "TaksitAySayisi");

            migrationBuilder.AddColumn<int>(
                name: "BaslangicAyi",
                table: "OkulUcretleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaslangicAyi",
                table: "OkulUcretleri");

            migrationBuilder.RenameColumn(
                name: "TaksitAySayisi",
                table: "OkulUcretleri",
                newName: "TaksitSayisi");
        }
    }
}
