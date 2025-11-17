using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagerCom.Migrations
{
    /// <inheritdoc />
    public partial class coloumnrateProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "Product",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Product");
        }
    }
}
