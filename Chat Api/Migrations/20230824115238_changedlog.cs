using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat_Api.Migrations
{
    /// <inheritdoc />
    public partial class changedlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "logs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "logs",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "logs",
                newName: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "logs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "logs",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "logs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
