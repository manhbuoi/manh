using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahanggiay.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentAndShipper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ShipperId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipperId",
                table: "Orders",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ShoeId",
                table: "OrderItems",
                column: "ShoeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Shoe_ShoeId",
                table: "OrderItems",
                column: "ShoeId",
                principalTable: "Shoe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_ShipperId",
                table: "Orders",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Shoe_ShoeId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ShipperId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipperId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ShoeId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "Orders");
        }
    }
}
