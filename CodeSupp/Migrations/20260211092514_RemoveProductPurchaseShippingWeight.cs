using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSupp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductPurchaseShippingWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCostPerKg",
                table: "ProductPurchaseHistories");

            migrationBuilder.DropColumn(
                name: "TotalKg",
                table: "ProductPurchaseHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCostPerKg",
                table: "ProductPurchaseHistories",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalKg",
                table: "ProductPurchaseHistories",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
