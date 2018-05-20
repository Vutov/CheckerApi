using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CheckerApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Data",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AcceptedSpeed = table.Column<double>(nullable: false),
                    Algo = table.Column<string>(nullable: true),
                    Alive = table.Column<bool>(nullable: false),
                    LimitSpeed = table.Column<double>(nullable: false),
                    NiceHashId = table.Column<string>(nullable: true),
                    Price = table.Column<double>(nullable: false),
                    RecordDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Workers = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Data");
        }
    }
}
