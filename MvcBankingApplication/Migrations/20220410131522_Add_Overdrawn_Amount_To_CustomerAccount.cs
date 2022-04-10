using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBankingApplication.Migrations
{
    public partial class Add_Overdrawn_Amount_To_CustomerAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OverdrawnAmount",
                table: "AccountModel",
                type: "REAL",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverdrawnAmount",
                table: "AccountModel");
        }
    }
}
