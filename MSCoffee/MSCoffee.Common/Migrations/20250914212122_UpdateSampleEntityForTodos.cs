using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSCoffee.Common.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSampleEntityForTodos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                schema: "public",
                table: "samples",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                schema: "public",
                table: "samples");
        }
    }
}
