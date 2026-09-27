using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class BransKaldirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branş",
                table: "Ogretmenler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branş",
                table: "Ogretmenler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
