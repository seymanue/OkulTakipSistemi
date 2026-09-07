using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ToplamUcretVeTaksitSayisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AylikUcret",
                table: "OkulUcretleri",
                newName: "ToplamUcret");

            migrationBuilder.AddColumn<int>(
                name: "TaksitSayisi",
                table: "OkulUcretleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaksitSayisi",
                table: "OkulUcretleri");

            migrationBuilder.RenameColumn(
                name: "ToplamUcret",
                table: "OkulUcretleri",
                newName: "AylikUcret");
        }
    }
}
