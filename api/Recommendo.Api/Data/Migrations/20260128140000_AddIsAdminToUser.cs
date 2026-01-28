using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recommendo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Set IsAdmin for the admin email
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"IsAdmin\" = true WHERE \"Email\" = 'ryancoates89@hotmail.co.uk'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");
        }
    }
}
