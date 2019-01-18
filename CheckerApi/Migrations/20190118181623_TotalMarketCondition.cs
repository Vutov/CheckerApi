using Microsoft.EntityFrameworkCore.Migrations;

namespace CheckerApi.Migrations
{
    public partial class TotalMarketCondition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalHashThreshold",
                table: "Configurations",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalHashThreshold",
                table: "Configurations");
        }
    }
}
