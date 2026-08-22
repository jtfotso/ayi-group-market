using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AYIGroupMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCustomerConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerConfirmedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerConfirmedAt",
                table: "Orders");
        }
    }
}
