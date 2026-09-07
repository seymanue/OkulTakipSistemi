using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OkulTakipSistemi.Data;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260906120000_UcretYonetimiYapisi")]
    public partial class UcretYonetimiYapisi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KurumIcinBelirlenenUcret",
                table: "OkulUcretleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StandartVeliUcreti",
                table: "OkulUcretleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OgrenciOzelUcretler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OgrenciKaydiId = table.Column<int>(type: "INTEGER", nullable: false),
                    OzelUcret = table.Column<decimal>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciOzelUcretler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciOzelUcretler_OgrenciKayitlari_OgrenciKaydiId",
                        column: x => x.OgrenciKaydiId,
                        principalTable: "OgrenciKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciOzelUcretler_OgrenciKaydiId",
                table: "OgrenciOzelUcretler",
                column: "OgrenciKaydiId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OgrenciOzelUcretler");

            migrationBuilder.DropColumn(
                name: "KurumIcinBelirlenenUcret",
                table: "OkulUcretleri");

            migrationBuilder.DropColumn(
                name: "StandartVeliUcreti",
                table: "OkulUcretleri");
        }
    }
}
