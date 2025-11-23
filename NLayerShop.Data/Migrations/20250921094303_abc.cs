using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLayerShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class abc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemoveCategory",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RemoveCategory",
                table: "Products",
                type: "bit",
                nullable: true);
        }
    }
}
