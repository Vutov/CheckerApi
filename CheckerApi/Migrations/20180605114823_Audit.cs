using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CheckerApi.Migrations
{
    public partial class Audit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderAudits",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    AcceptedSpeed = table.Column<double>(nullable: false),
                    Algo = table.Column<string>(nullable: true),
                    Alive = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    LimitSpeed = table.Column<double>(nullable: false),
                    NiceHashDataCenter = table.Column<int>(nullable: false),
                    NiceHashId = table.Column<string>(nullable: true),
                    Price = table.Column<double>(nullable: false),
                    RecordDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Workers = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAudits", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAudits");
        }
    }
}
