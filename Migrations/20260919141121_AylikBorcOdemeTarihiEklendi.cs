using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AylikBorcOdemeTarihiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OdemeTarihi",
                table: "AylikBorclar",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OdemeTarihi",
                table: "AylikBorclar");
        }
    }
}
