using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OgrenciKisiselBilgileriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Ogrenciler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cinsiyet",
                table: "Ogrenciler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DogumTarihi",
                table: "Ogrenciler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KayitTarihi",
                table: "Ogrenciler",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Ogrenciler",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "Cinsiyet",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "DogumTarihi",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "KayitTarihi",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "Telefon",
                table: "Ogrenciler");
        }
    }
}
