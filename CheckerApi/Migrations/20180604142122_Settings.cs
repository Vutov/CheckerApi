using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CheckerApi.Migrations
{
    public partial class Settings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinimalAcceptedSpeed",
                table: "Configurations",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ConditionSettings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    ConditionID = table.Column<int>(nullable: false),
                    ConditionName = table.Column<string>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionSettings", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConditionSettings");

            migrationBuilder.DropColumn(
                name: "MinimalAcceptedSpeed",
                table: "Configurations");
        }
    }
}
