using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat_Api.Migrations
{
    /// <inheritdoc />
    public partial class changefields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "logs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "logs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
