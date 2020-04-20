using Microsoft.EntityFrameworkCore.Migrations;

namespace Thandizo.IdentityServer.Data.Migrations.AspNetIdentity
{
    public partial class AddDefaultPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultPassword",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPassword",
                table: "AspNetUsers");
        }
    }
}
