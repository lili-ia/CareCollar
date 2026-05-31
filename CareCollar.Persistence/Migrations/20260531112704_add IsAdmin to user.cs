using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareCollar.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addIsAdmintouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_admin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_admin",
                table: "users");
        }
    }
}
