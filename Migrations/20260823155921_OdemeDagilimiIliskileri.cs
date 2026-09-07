using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class OdemeDagilimiIliskileri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OdemeDagilimi_AylikBorclar_AylikBorcId",
                table: "OdemeDagilimi");

            migrationBuilder.DropForeignKey(
                name: "FK_OdemeDagilimi_Odemeler_OdemeId",
                table: "OdemeDagilimi");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OdemeDagilimi",
                table: "OdemeDagilimi");

            migrationBuilder.RenameTable(
                name: "OdemeDagilimi",
                newName: "OdemeDagilimlari");

            migrationBuilder.RenameIndex(
                name: "IX_OdemeDagilimi_OdemeId",
                table: "OdemeDagilimlari",
                newName: "IX_OdemeDagilimlari_OdemeId");

            migrationBuilder.RenameIndex(
                name: "IX_OdemeDagilimi_AylikBorcId",
                table: "OdemeDagilimlari",
                newName: "IX_OdemeDagilimlari_AylikBorcId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OdemeDagilimlari",
                table: "OdemeDagilimlari",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OdemeDagilimlari_AylikBorclar_AylikBorcId",
                table: "OdemeDagilimlari",
                column: "AylikBorcId",
                principalTable: "AylikBorclar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OdemeDagilimlari_Odemeler_OdemeId",
                table: "OdemeDagilimlari",
                column: "OdemeId",
                principalTable: "Odemeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OdemeDagilimlari_AylikBorclar_AylikBorcId",
                table: "OdemeDagilimlari");

            migrationBuilder.DropForeignKey(
                name: "FK_OdemeDagilimlari_Odemeler_OdemeId",
                table: "OdemeDagilimlari");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OdemeDagilimlari",
                table: "OdemeDagilimlari");

            migrationBuilder.RenameTable(
                name: "OdemeDagilimlari",
                newName: "OdemeDagilimi");

            migrationBuilder.RenameIndex(
                name: "IX_OdemeDagilimlari_OdemeId",
                table: "OdemeDagilimi",
                newName: "IX_OdemeDagilimi_OdemeId");

            migrationBuilder.RenameIndex(
                name: "IX_OdemeDagilimlari_AylikBorcId",
                table: "OdemeDagilimi",
                newName: "IX_OdemeDagilimi_AylikBorcId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OdemeDagilimi",
                table: "OdemeDagilimi",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OdemeDagilimi_AylikBorclar_AylikBorcId",
                table: "OdemeDagilimi",
                column: "AylikBorcId",
                principalTable: "AylikBorclar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OdemeDagilimi_Odemeler_OdemeId",
                table: "OdemeDagilimi",
                column: "OdemeId",
                principalTable: "Odemeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
