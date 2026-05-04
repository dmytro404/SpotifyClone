using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyClone.Migrations
{
    /// <inheritdoc />
    public partial class GuestCanRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: "Guest",
                column: "CanRead",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: "Guest",
                column: "CanRead",
                value: false);
        }
    }
}
