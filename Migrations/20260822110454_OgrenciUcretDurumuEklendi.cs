using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OgrenciUcretDurumuEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciUcretDurumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OgrenciKaydiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Indirimli = table.Column<bool>(type: "INTEGER", nullable: false),
                    IndirimOrani = table.Column<decimal>(type: "TEXT", nullable: false),
                    OzelFiyat = table.Column<decimal>(type: "TEXT", nullable: true),
                    Ucretsiz = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciUcretDurumlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciUcretDurumlari_OgrenciKayitlari_OgrenciKaydiId",
                        column: x => x.OgrenciKaydiId,
                        principalTable: "OgrenciKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciUcretDurumlari_OgrenciKaydiId",
                table: "OgrenciUcretDurumlari",
                column: "OgrenciKaydiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciUcretDurumlari");
        }
    }
}
