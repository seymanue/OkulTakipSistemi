using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OgrenciKaydiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OgrenciId = table.Column<int>(type: "INTEGER", nullable: false),
                    OkulId = table.Column<int>(type: "INTEGER", nullable: false),
                    SinifId = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciKayitlari_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OgrenciKayitlari_Okullar_OkulId",
                        column: x => x.OkulId,
                        principalTable: "Okullar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciKayitlari_Siniflar_SinifId",
                        column: x => x.SinifId,
                        principalTable: "Siniflar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciKayitlari_OgrenciId",
                table: "OgrenciKayitlari",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciKayitlari_OkulId",
                table: "OgrenciKayitlari",
                column: "OkulId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciKayitlari_SinifId",
                table: "OgrenciKayitlari",
                column: "SinifId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciKayitlari");
        }
    }
}
