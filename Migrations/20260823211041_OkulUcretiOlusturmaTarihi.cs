using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OkulUcretiOlusturmaTarihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "OkulUcretleri",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OlusturmaTarihi",
                table: "OkulUcretleri");
        }
    }
}
