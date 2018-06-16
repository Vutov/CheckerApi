using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CheckerApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConditionSettings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConditionID = table.Column<int>(nullable: false),
                    ConditionName = table.Column<string>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionSettings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AcceptedSpeed = table.Column<double>(nullable: false),
                    LimitSpeed = table.Column<double>(nullable: false),
                    PriceThreshold = table.Column<double>(nullable: false),
                    LastNotification = table.Column<DateTime>(nullable: false),
                    MinimalAcceptedSpeed = table.Column<double>(nullable: false),
                    AcceptedPercentThreshold = table.Column<double>(nullable: false),
                    EnableAudit = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Data",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecordDate = table.Column<DateTime>(nullable: false),
                    LimitSpeed = table.Column<double>(nullable: false),
                    Alive = table.Column<bool>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    NiceHashId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Workers = table.Column<string>(nullable: true),
                    Algo = table.Column<string>(nullable: true),
                    AcceptedSpeed = table.Column<double>(nullable: false),
                    NiceHashDataCenter = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "OrderAudits",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecordDate = table.Column<DateTime>(nullable: false),
                    LimitSpeed = table.Column<double>(nullable: false),
                    Alive = table.Column<bool>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    NiceHashId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Workers = table.Column<string>(nullable: true),
                    Algo = table.Column<string>(nullable: true),
                    AcceptedSpeed = table.Column<double>(nullable: false),
                    NiceHashDataCenter = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAudits", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConditionSettings");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Data");

            migrationBuilder.DropTable(
                name: "OrderAudits");
        }
    }
}
