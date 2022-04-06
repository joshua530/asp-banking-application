using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBankingApplication.Migrations
{
    public partial class Autogenerate_User_DateCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2022, 4, 6, 12, 41, 15, 870, DateTimeKind.Local).AddTicks(3680));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2022, 4, 6, 12, 41, 15, 870, DateTimeKind.Local).AddTicks(3680),
                oldClrType: typeof(DateTime),
                oldType: "TEXT");
        }
    }
}
