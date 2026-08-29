using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AYIGroupMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionCustomerTargeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetCustomerEmail",
                table: "Promotions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetCustomerPhone",
                table: "Promotions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetCustomerEmail",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "TargetCustomerPhone",
                table: "Promotions");
        }
    }
}
