using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLoggingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                table: "ApplicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionType",
                table: "ApplicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ApplicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "ApplicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "ExceptionType",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "StackTrace",
                table: "ApplicationLogs");
        }
    }
}
