using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonShared.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTokenAndMessengerIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessengerId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MessengerId",
                table: "Users");
        }
    }
}
