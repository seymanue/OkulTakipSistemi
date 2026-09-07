using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class EmailDogrulamaEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DogrulamaKodu",
                table: "Kullanicilar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DogrulamaKoduSonKullanma",
                table: "Kullanicilar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailDogrulandi",
                table: "Kullanicilar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DogrulamaKodu",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "DogrulamaKoduSonKullanma",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "EmailDogrulandi",
                table: "Kullanicilar");
        }
    }
}
