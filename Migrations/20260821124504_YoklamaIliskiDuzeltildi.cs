using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkulTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class YoklamaIliskiDuzeltildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YoklamaId1",
                table: "YoklamaDetaylari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YoklamaDetaylari_YoklamaId1",
                table: "YoklamaDetaylari",
                column: "YoklamaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_YoklamaDetaylari_Yoklamalar_YoklamaId1",
                table: "YoklamaDetaylari",
                column: "YoklamaId1",
                principalTable: "Yoklamalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YoklamaDetaylari_Yoklamalar_YoklamaId1",
                table: "YoklamaDetaylari");

            migrationBuilder.DropIndex(
                name: "IX_YoklamaDetaylari_YoklamaId1",
                table: "YoklamaDetaylari");

            migrationBuilder.DropColumn(
                name: "YoklamaId1",
                table: "YoklamaDetaylari");
        }
    }
}
