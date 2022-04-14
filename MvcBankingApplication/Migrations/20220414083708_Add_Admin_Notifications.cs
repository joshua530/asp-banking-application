using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBankingApplication.Migrations
{
    public partial class Add_Admin_Notifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Notifications");
        }
    }
}
