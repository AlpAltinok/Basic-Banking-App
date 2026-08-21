using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Wallets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Wallets");
        }
    }
}
