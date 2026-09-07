using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OkulUcretiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OkulUcretleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OkulId = table.Column<int>(type: "INTEGER", nullable: false),
                    AylikUcret = table.Column<decimal>(type: "TEXT", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OkulUcretleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OkulUcretleri_Okullar_OkulId",
                        column: x => x.OkulId,
                        principalTable: "Okullar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OkulUcretleri_OkulId",
                table: "OkulUcretleri",
                column: "OkulId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OkulUcretleri");
        }
    }
}
