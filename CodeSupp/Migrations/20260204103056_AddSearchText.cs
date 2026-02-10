using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSupp.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Products",
                type: "citext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Products");
        }
    }
}
