using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfigService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyAppApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Applications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Applications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
