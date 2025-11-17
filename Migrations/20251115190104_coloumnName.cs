using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagerCom.Migrations
{
    /// <inheritdoc />
    public partial class coloumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
          name: "Name",
               table: "Vendor",
                type: "nvarchar(max)",
                  nullable: false,
                    defaultValue: ""); // ممكن تحدد قيمة افتراضية لو محتاج

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
               name: "Name",
                  table: "Vendor");

        }
    }
}
