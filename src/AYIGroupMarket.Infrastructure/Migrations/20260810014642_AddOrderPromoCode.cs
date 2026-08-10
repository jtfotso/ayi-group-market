using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AYIGroupMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPromoCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromoCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromoCode",
                table: "Orders");
        }
    }
}
