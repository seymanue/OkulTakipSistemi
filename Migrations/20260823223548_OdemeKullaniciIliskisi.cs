using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OdemeKullaniciIliskisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<int>(
                name: "KullaniciId",
    table: "Odemeler",
    type: "INTEGER",
    nullable: false,
    defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Odemeler_KullaniciId",
                table: "Odemeler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Odemeler_Kullanicilar_KullaniciId",
                table: "Odemeler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Odemeler_Kullanicilar_KullaniciId",
                table: "Odemeler");

            migrationBuilder.DropIndex(
                name: "IX_Odemeler_KullaniciId",
                table: "Odemeler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Odemeler");
        }
    }
}
