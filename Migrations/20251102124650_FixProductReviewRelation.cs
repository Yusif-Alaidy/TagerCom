using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagerCom.Migrations
{
    /// <inheritdoc />
    public partial class FixProductReviewRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_CustomerId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId1",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_ProductId1",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "Review");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_CustomerId",
                table: "Review",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_CustomerId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "Review",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_ProductId1",
                table: "Review",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_CustomerId",
                table: "Review",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId1",
                table: "Review",
                column: "ProductId1",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
