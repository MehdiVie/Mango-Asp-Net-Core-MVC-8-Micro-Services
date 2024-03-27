using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Services.OrderAPI.Migrations
{
    /// <inheritdoc />
    public partial class addOrderTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartDetails_CartHeader_OrderHeaderId",
                table: "CartDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartHeader",
                table: "CartHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartDetails",
                table: "CartDetails");

            migrationBuilder.RenameTable(
                name: "CartHeader",
                newName: "OrderHeader");

            migrationBuilder.RenameTable(
                name: "CartDetails",
                newName: "OrderDetails");

            migrationBuilder.RenameIndex(
                name: "IX_CartDetails_OrderHeaderId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderHeader",
                table: "OrderHeader",
                column: "OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails",
                column: "OrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_OrderHeader_OrderHeaderId",
                table: "OrderDetails",
                column: "OrderHeaderId",
                principalTable: "OrderHeader",
                principalColumn: "OrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_OrderHeader_OrderHeaderId",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderHeader",
                table: "OrderHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails");

            migrationBuilder.RenameTable(
                name: "OrderHeader",
                newName: "CartHeader");

            migrationBuilder.RenameTable(
                name: "OrderDetails",
                newName: "CartDetails");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_OrderHeaderId",
                table: "CartDetails",
                newName: "IX_CartDetails_OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartHeader",
                table: "CartHeader",
                column: "OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartDetails",
                table: "CartDetails",
                column: "OrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetails_CartHeader_OrderHeaderId",
                table: "CartDetails",
                column: "OrderHeaderId",
                principalTable: "CartHeader",
                principalColumn: "OrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
