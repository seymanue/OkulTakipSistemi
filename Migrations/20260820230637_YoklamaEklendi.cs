using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class YoklamaEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Yoklamalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SinifId = table.Column<int>(type: "INTEGER", nullable: false),
                    OgretmenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yoklamalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Yoklamalar_Ogretmenler_OgretmenId",
                        column: x => x.OgretmenId,
                        principalTable: "Ogretmenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Yoklamalar_Siniflar_SinifId",
                        column: x => x.SinifId,
                        principalTable: "Siniflar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YoklamaDetaylari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    YoklamaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OgrenciId = table.Column<int>(type: "INTEGER", nullable: false),
                    Geldi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YoklamaDetaylari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YoklamaDetaylari_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YoklamaDetaylari_Yoklamalar_YoklamaId",
                        column: x => x.YoklamaId,
                        principalTable: "Yoklamalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YoklamaDetaylari_OgrenciId",
                table: "YoklamaDetaylari",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_YoklamaDetaylari_YoklamaId",
                table: "YoklamaDetaylari",
                column: "YoklamaId");

            migrationBuilder.CreateIndex(
                name: "IX_Yoklamalar_OgretmenId",
                table: "Yoklamalar",
                column: "OgretmenId");

            migrationBuilder.CreateIndex(
                name: "IX_Yoklamalar_SinifId",
                table: "Yoklamalar",
                column: "SinifId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YoklamaDetaylari");

            migrationBuilder.DropTable(
                name: "Yoklamalar");
        }
    }
}
